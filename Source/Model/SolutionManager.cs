using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Reflection;
using EnvDTE;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Threading;
using Alphaleonis.VSProjectSetMgr;

namespace Alphaleonis.VSProjectSetMgr
{
   public enum ProjectOptions
   {
      All = __VSENUMPROJFLAGS.EPF_ALLINSOLUTION,
      Unloaded = __VSENUMPROJFLAGS.EPF_UNLOADEDINSOLUTION,
      Loaded = __VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION
   }

   interface IProjectDescriptor : IEquatable<IProjectDescriptor>
   {
      Guid Id { get; }
      string Name { get; }
      Guid Kind { get; }
   }

   class ProjectDescriptor : IProjectDescriptor
   {
      private readonly Guid m_id;
      private readonly string m_name;
      private readonly Guid m_kind;

      public ProjectDescriptor(Guid id, string name, Guid kind)
      {
         m_id = id;
         m_name = name;
         m_kind = kind;
      }

      public Guid Id
      {
         get
         {
            return m_id;
         }
      }

      public string Name
      {
         get
         {
            return m_name;
         }
      }

      public Guid Kind
      {
         get
         {
            return m_kind;
         }
      }

      public bool Equals(IProjectDescriptor other)
      {
         return Id.Equals(other.Id);
      }

      public override bool Equals(object obj)
      {
         return Equals(obj as ProjectDescriptor);
      }

      public override int GetHashCode()
      {
         return Id.GetHashCode();
      }

      public override string ToString()
      {
         return string.Format("Project {0}", Name);
      }
   }

   public class SolutionInfo
   {
      private readonly string m_solutionDirectory;
      private readonly string m_solutionFile;
      private readonly string m_solutionOptionsFile;

      public SolutionInfo(string solutionDirectory, string solutionFile, string solutionOptionsFile)
      {
         m_solutionDirectory = solutionDirectory;
         m_solutionFile = solutionFile;
         m_solutionOptionsFile = solutionOptionsFile;
      }

      public string SolutionDirectory
      {
         get
         {
            return m_solutionDirectory;
         }
      }

      public string SolutionFile
      {
         get
         {
            return m_solutionFile;
         }
      }

      public string SolutionOptionsFile
      {
         get
         {
            return m_solutionOptionsFile;
         }
      }
   }

   class SolutionManager
   {
      #region Private Fields

      private readonly static Guid m_outputPaneGuid = new Guid("BF899951-951D-4CC6-9D57-8C42C710BE35");
      private readonly static Guid m_solutionFolderId = new Guid("{66A26720-8FB5-11D2-AA7E-00C04F688DDE}");
      private readonly IVsSolution m_solution;
      private readonly IVsSolution4 m_solution4;
      private readonly IOutputWindow m_outputWindow;

      #endregion

      public SolutionManager(IVsSolution solution, IOutputWindow outputWindow)
      {
         if (solution == null)
            throw new ArgumentNullException("solution", "solution is null.");

         m_solution = solution;
         m_solution4 = (IVsSolution4)solution;
         m_outputWindow = outputWindow;
      }

      #region Public Methods

      private void WriteLog(string message)
      {         
         m_outputWindow.GetOrCreatePane(m_outputPaneGuid, "Project Set Manager", false).WriteLine(message);
      }

      public SolutionInfo GetSolutionInfo()
      {
         string solOptsFile;
         string solFile;
         string solDir;
         ErrorHandler.ThrowOnFailure(m_solution.GetSolutionInfo(out solDir, out solFile, out solOptsFile));
         return new SolutionInfo(solDir, solFile, solOptsFile);
      }

      public IEnumerable<ProjectDescriptor> GetProjects(ProjectOptions options, bool includeMiscProjectsAndSolutionFolders = false)
      {
         IEnumHierarchies ppEnum;
         Guid tempGuid = Guid.Empty;
         ErrorHandler.ThrowOnFailure(m_solution.GetProjectEnum((uint)options, ref tempGuid, out ppEnum));


         if (ppEnum != null)
         {
            uint actualResult = 0;
            IVsHierarchy[] nodes = new IVsHierarchy[1];
            while (0 == ppEnum.Next(1, nodes, out actualResult))
            {
               Guid projectId;
               object projectName;
               ErrorHandler.ThrowOnFailure(nodes[0].GetGuidProperty((uint)Microsoft.VisualStudio.VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out projectId));
               ErrorHandler.ThrowOnFailure(nodes[0].GetProperty((uint)Microsoft.VisualStudio.VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ProjectName, out projectName));

               Guid projectTypeId;
               Guid projectKind = Guid.Empty;

               if (ErrorHandler.Succeeded(nodes[0].GetGuidProperty(Microsoft.VisualStudio.VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_TypeGuid, out projectTypeId)))
               {
                  object pVar;
                  if (ErrorHandler.Succeeded(nodes[0].GetProperty(Microsoft.VisualStudio.VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out pVar)))
                  {
                     Project project = pVar as Project;
                     if (project != null)
                        Guid.TryParse(project.Kind, out projectKind);
                  }
               }

               // Solution Folder: {66A26720-8FB5-11D2-AA7E-00C04F688DDE}
               // If the project is actually a solution folder and we have elected to skip those, then skip it.
               if (!includeMiscProjectsAndSolutionFolders)
               {
                  if (projectKind == Guid.Parse(EnvDTE.Constants.vsProjectKindSolutionItems) ||
                      projectKind == Guid.Parse(EnvDTE.Constants.vsProjectKindMisc))
                  {
                     continue;
                  }
               }


               //object pImage;
               //nodes[0].GetProperty(Microsoft.VisualStudio.VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_IconHandle, out pImage);
               //IntPtr ipImage = IntPtr.Zero;
               //if (pImage != null)
               //{
               //   ipImage = new IntPtr((int)pImage);
               //}
               yield return new ProjectDescriptor(projectId, (string)projectName, projectKind);
            }
         }
      }

      public void UnloadProject(ProjectDescriptor project)
      {
         Guid projectId = project.Id;
         WriteLog($"Unloading project \"{project.Name ?? project.Id.ToString()}\"");
         try
         {
            ErrorHandler.ThrowOnFailure(m_solution4.UnloadProject(ref projectId, (uint)_VSProjectUnloadStatus.UNLOADSTATUS_UnloadedByUser));
         }
         catch (Exception ex)
         {
            WriteLog($"Error: Failed to unload project \"{project.Name ?? project.Id.ToString()}\"; {ex.Message}");
         }
      }

      public void LoadProject(ProjectDescriptor project)
      {
         Guid projectId = project.Id;
         WriteLog($"Loading project \"{project.Name ?? project.Id.ToString()}\"");
         try
         {
            ErrorHandler.ThrowOnFailure(m_solution4.ReloadProject(ref projectId));
         }
         catch (Exception ex)
         {
            WriteLog($"Error: Failed to load project \"{project.Name ?? project.Id.ToString()}\"; {ex.Message}");
         }
      }

      public void UnloadExclusive(ISet<Guid> projects)
      {
         foreach (ProjectDescriptor project in GetProjects(ProjectOptions.Unloaded, false).Where(p => !projects.Contains(p.Id)))
            LoadProject(project);

         foreach (var project in GetProjects(ProjectOptions.Loaded, false).Where(p => projects.Contains(p.Id)))
            UnloadProject(project);
      }

      public void LoadExclusive(ISet<Guid> projects)
      {
         foreach (var project in GetProjects(ProjectOptions.Loaded, false).Where(p => !projects.Contains(p.Id)))
            UnloadProject(project);

         foreach (var project in GetProjects(ProjectOptions.Unloaded, false).Where(p => projects.Contains(p.Id)))
            LoadProject(project);
      }

      public void Unload(ISet<Guid> projects)
      {
         foreach (var project in GetProjects(ProjectOptions.Loaded, false).Where(p => projects.Contains(p.Id)))
            UnloadProject(project);
      }

      public void Load(ISet<Guid> projects)
      {
         foreach (var project in GetProjects(ProjectOptions.Unloaded, false).Where(p => projects.Contains(p.Id)))
         {
            LoadProject(project);
         }
      }


      public void SaveUserOpts()
      {
         m_solution4.WriteUserOptsFile();
      }

      public ISolutionHierarchyContainerItem GetSolutionHierarchy(bool visibleNodesOnly = false)
      {
         ISolutionHierarchyContainerItem solution = CreateSolutionHierarchyItem((IVsHierarchy)m_solution, (uint)Microsoft.VisualStudio.VSConstants.VSITEMID_ROOT) as ISolutionHierarchyContainerItem;
         if (solution != null)
            PopulateHierarchy(solution.VsHierarchy, solution.HierarchyItemId, visibleNodesOnly, solution, solution);

         return solution;
      }

      public ISolutionHierarchyItem CreateSolutionHierarchyItem(IVsHierarchy hierarchy, uint itemId)
      {
         int hr;
         IntPtr nestedHierarchyObj;
         uint nestedItemId;
         Guid hierGuid = typeof(IVsHierarchy).GUID;

         // Check first if this node has a nested hierarchy. If so, then there really are two 
         // identities for this node: 1. hierarchy/itemid 2. nestedHierarchy/nestedItemId.
         // We will recurse and call EnumHierarchyItems which will display this node using
         // the inner nestedHierarchy/nestedItemId identity.
         hr = hierarchy.GetNestedHierarchy(itemId, ref hierGuid, out nestedHierarchyObj, out nestedItemId);
         if (VSConstants.S_OK == hr && IntPtr.Zero != nestedHierarchyObj)
         {
            IVsHierarchy nestedHierarchy = Marshal.GetObjectForIUnknown(nestedHierarchyObj) as IVsHierarchy;
            Marshal.Release(nestedHierarchyObj);    // we are responsible to release the refcount on the out IntPtr parameter
            if (nestedHierarchy != null)
            {
               // Display name and type of the node in the Output Window
               return CreateSolutionHierarchyItem(nestedHierarchy, nestedItemId);
            }

            return null;
         }
         else
         {
            return CreateSolutionHierarchyItemDirect(hierarchy, itemId);
         }
      }

      #endregion

      #region Private Methods

      private static uint GetItemId(object pvar)
      {
         if (pvar == null) return VSConstants.VSITEMID_NIL;
         if (pvar is int) return (uint)(int)pvar;
         if (pvar is uint) return (uint)pvar;
         if (pvar is short) return (uint)(short)pvar;
         if (pvar is ushort) return (uint)(ushort)pvar;
         if (pvar is long) return (uint)(long)pvar;
         return VSConstants.VSITEMID_NIL;
      }

      private static bool IsSolutionFolder(IVsHierarchy hierarchy, uint itemId)
      {
         object pVar;
         hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out pVar);
         Guid kindId;
         return pVar != null && pVar is Project && Guid.TryParse(((Project)pVar).Kind, out kindId) && kindId == m_solutionFolderId;
      }

      private static ISolutionHierarchyItem CreateSolutionHierarchyItemDirect(IVsHierarchy hierarchy, uint itemId)
      {
         object pVar;
         Guid id;
         ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_Name, out pVar));
         string projectName = (string)pVar;

         int hr = hierarchy.GetGuidProperty(itemId, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out id);
         hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out pVar);
         if (pVar == null)
         {
            try
            {
               if (ErrorHandler.Succeeded(hierarchy.GetProperty(itemId, (int)__VSHPROPID5.VSHPROPID_ProjectUnloadStatus, out pVar)))
               {
                  return new SolutionHierarchyItem(hierarchy, itemId, SolutionHierarchyItemType.UnloadedProject);
               }
            }
            catch
            {
            }

            return null;
         }
         else
         {
            Project pr = pVar as Project;
            if (pr != null)
            {
               Guid projectKind;
               if (Guid.TryParse(pr.Kind, out projectKind) && projectKind == m_solutionFolderId)
               {
                  return new SolutionHierarchyContainerItem(hierarchy, itemId, SolutionHierarchyItemType.SolutionFolder);
               }
               else
               {
                  return new SolutionHierarchyItem(hierarchy, itemId, SolutionHierarchyItemType.Project);
               }
            }
            else
            {
               Solution sol = pVar as Solution;
               if (sol == null)
               {
                  // Unknown item
                  return null;
               }
               else
               {
                  // Solution
                  return new SolutionHierarchyContainerItem(hierarchy, itemId, SolutionHierarchyItemType.Solution);
               }
            }
         }
      }

      private ISolutionHierarchyItem PopulateHierarchy(IVsHierarchy hierarchy, uint itemId, bool visibleNodesOnly, ISolutionHierarchyContainerItem solutionNode, ISolutionHierarchyItem thisNode)
      {
         ISolutionHierarchyContainerItem container = thisNode as ISolutionHierarchyContainerItem;

         if (container != null)
         {
            if (solutionNode == null)
               solutionNode = container;

            IEnumerable<uint> childIds = EnumerateChildIds(hierarchy, itemId, visibleNodesOnly);

            List<ISolutionHierarchyItem> list = new List<ISolutionHierarchyItem>();

            foreach (uint childId in childIds)
            {
               ISolutionHierarchyItem childItem = CreateSolutionHierarchyItem(hierarchy, childId);
               if (childItem != null)
               {
                  container.Children.Add(childItem);
                  list.Add(childItem);

                  // Due to a bug in VS, enumerating the solution node actually returns all projects within the solution, at any depth,
                  // so we need to remove them here if found.
                  if (container != solutionNode)
                  {
                     solutionNode.Children.Remove(childItem.Id);
                  }
               }
            }

            foreach (var childItem in list.OfType<ISolutionHierarchyContainerItem>())
            {
               if (container.Children.Contains(childItem.Id)) // On SolutionNode the child may actually have been removed (see below)
               {
                  PopulateHierarchy(childItem.VsHierarchy, childItem.HierarchyItemId, visibleNodesOnly, solutionNode, childItem);
               }
            }
         }
         return thisNode;

      }

      private ISolutionHierarchyItem GetSolutionHierarchy(IVsHierarchy hierarchy, uint itemId, bool visibleNodesOnly, ISolutionHierarchyContainerItem solutionNode)
      {
         int hr;
         IntPtr nestedHierarchyObj;
         uint nestedItemId;
         Guid hierGuid = typeof(IVsHierarchy).GUID;

         // Check first if this node has a nested hierarchy. If so, then there really are two 
         // identities for this node: 1. hierarchy/itemid 2. nestedHierarchy/nestedItemId.
         // We will recurse and call EnumHierarchyItems which will display this node using
         // the inner nestedHierarchy/nestedItemId identity.
         hr = hierarchy.GetNestedHierarchy(itemId, ref hierGuid, out nestedHierarchyObj, out nestedItemId);
         if (VSConstants.S_OK == hr && IntPtr.Zero != nestedHierarchyObj)
         {
            IVsHierarchy nestedHierarchy = Marshal.GetObjectForIUnknown(nestedHierarchyObj) as IVsHierarchy;
            Marshal.Release(nestedHierarchyObj);    // we are responsible to release the refcount on the out IntPtr parameter
            if (nestedHierarchy != null)
            {
               // Display name and type of the node in the Output Window
               return GetSolutionHierarchy(nestedHierarchy, nestedItemId, visibleNodesOnly, solutionNode);
            }

            return null;
         }
         else
         {
            ISolutionHierarchyItem item = CreateSolutionHierarchyItemDirect(hierarchy, itemId);
            ISolutionHierarchyContainerItem container = item as ISolutionHierarchyContainerItem;

            if (container != null)
            {
               if (solutionNode == null)
                  solutionNode = container;

               IEnumerable<uint> childIds = EnumerateChildIds(hierarchy, itemId, visibleNodesOnly);

               if (container == solutionNode && solutionNode != null)
                  childIds = childIds.OrderBy(id => IsSolutionFolder(hierarchy, id) ? 1 : 0);

               foreach (uint childId in childIds)
               {
                  ISolutionHierarchyItem childItem = GetSolutionHierarchy(hierarchy, childId, visibleNodesOnly, solutionNode);
                  if (childItem != null)
                  {
                     container.Children.Add(childItem);

                     // Due to a bug in VS, enumerating the solution node actually returns all projects within the solution, at any depth,
                     // so we need to remove them here if found.
                     if (container != solutionNode)
                        solutionNode.Children.Remove(childItem.Id);
                  }
               }
            }

            return item;
         }
      }

      private static IEnumerable<uint> EnumerateChildIds(IVsHierarchy hierarchy, uint itemId, bool visibleNodesOnly)
      {
         object pVar;
         int hr = hierarchy.GetProperty(itemId, ((visibleNodesOnly ? (int)__VSHPROPID.VSHPROPID_FirstVisibleChild : (int)__VSHPROPID.VSHPROPID_FirstChild)), out pVar);
         Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
         if (VSConstants.S_OK == hr)
         {
            uint childId = GetItemId(pVar);
            while (childId != VSConstants.VSITEMID_NIL)
            {
               yield return childId;

               hr = hierarchy.GetProperty(childId, visibleNodesOnly ? (int)__VSHPROPID.VSHPROPID_NextVisibleSibling : (int)__VSHPROPID.VSHPROPID_NextSibling, out pVar);
               if (VSConstants.S_OK == hr)
               {
                  childId = GetItemId(pVar);
               }
               else
               {
                  Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
                  break;
               }
            }
         }
      }

      #endregion
   }

   enum SolutionHierarchyItemType
   {
      Solution,
      SolutionFolder,
      Project,
      UnloadedProject
   }

   interface ISolutionHierarchyItem
   {
      Guid Id { get; }
      string Name { get; }
      IVsHierarchy VsHierarchy { get; }
      uint HierarchyItemId { get; }
      SolutionHierarchyItemType ItemType { get; }
      IntPtr ImageHandle { get; }
   }

   interface ISolutionHierarchyContainerItem : ISolutionHierarchyItem
   {
      KeyedCollection<Guid, ISolutionHierarchyItem> Children { get; }
   }

   class SolutionHierarchyContainerItem : SolutionHierarchyItem, ISolutionHierarchyContainerItem
   {
      private ChildCollection m_children;

      public SolutionHierarchyContainerItem(IVsHierarchy hierarchy, uint itemId, SolutionHierarchyItemType itemType)
         : base(hierarchy, itemId, itemType)
      {
      }

      public KeyedCollection<Guid, ISolutionHierarchyItem> Children
      {
         get
         {
            if (m_children == null)
            {
               m_children = new ChildCollection();
            }

            return m_children;
         }
      }


      private class ChildCollection : KeyedCollection<Guid, ISolutionHierarchyItem>
      {

         public ChildCollection()
         {
         }
         protected override Guid GetKeyForItem(ISolutionHierarchyItem item)
         {
            return item.Id;
         }
      }

   }

   class SolutionHierarchyItem : ISolutionHierarchyItem
   {
      #region Private Fields

      private readonly uint m_itemId;
      private Guid? m_id;
      private string m_name;
      private readonly IVsHierarchy m_hierarchy;
      private readonly SolutionHierarchyItemType m_itemType;

      #endregion

      public SolutionHierarchyItem(IVsHierarchy hierarchy, uint itemId, SolutionHierarchyItemType itemType)
      {
         m_itemId = itemId;
         m_hierarchy = hierarchy;
         m_itemType = itemType;
      }

      public IVsHierarchy VsHierarchy
      {
         get
         {
            return m_hierarchy;
         }
      }

      public uint HierarchyItemId
      {
         get
         {
            return m_itemId;
         }
      }

      public Guid Id
      {
         get
         {
            if (!m_id.HasValue)
            {
               Guid id;
               if (!ErrorHandler.Succeeded(m_hierarchy.GetGuidProperty(m_itemId, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out id)))
                  m_id = Guid.Empty;
               else
                  m_id = id;
            }

            return m_id.Value;
         }
      }

      public SolutionHierarchyItemType ItemType
      {
         get
         {
            return m_itemType;
         }
      }

      public string Name
      {
         get
         {
            if (m_name == null)
            {
               object pVar;
               ErrorHandler.ThrowOnFailure(m_hierarchy.GetProperty(m_itemId, (int)__VSHPROPID.VSHPROPID_Name, out pVar));
               m_name = (string)pVar;
            }
            return m_name;
         }
      }

      public IntPtr ImageHandle
      {
         get
         {
            object pVar;
            int hr = VsHierarchy.GetProperty(HierarchyItemId, (int)__VSHPROPID.VSHPROPID_IconHandle, out pVar);
            if (pVar != null && hr == 0)
            {
               return new IntPtr((int)pVar);
            }
            else
            {
               hr = VsHierarchy.GetProperty(HierarchyItemId, (int)__VSHPROPID.VSHPROPID_IconImgList, out pVar);
               if (hr == 0 && pVar != null)
               {
                  object index;
                  hr = VsHierarchy.GetProperty(HierarchyItemId, (int)__VSHPROPID.VSHPROPID_IconIndex, out index);
                  if (hr == 0 && index != null)
                     return NativeMethods.ImageList_GetIcon(new IntPtr((int)pVar), (int)index, 0);
               }
            }
            return IntPtr.Zero;
         }
      }

      private static class NativeMethods
      {
         [DllImport("comctl32.dll", CharSet = CharSet.None, ExactSpelling = false)]
         public static extern IntPtr ImageList_GetIcon(IntPtr imageListHandle, int iconIndex, int flags);
      }
   }

   public interface IProgressInfo
   {
      int PercentComplete { get; }
      string CurrentOperation { get; }
   }


}
