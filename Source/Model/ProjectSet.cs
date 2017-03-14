using Alphaleonis.VSProjectSetMgr.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Alphaleonis.VSProjectSetMgr
{
   class ProjectSet : ObservableBase
   {
      #region Private Fields

      private readonly Dictionary<Guid, bool> m_projects;
      private string m_name;

      #endregion

      public ProjectSet(BinaryReader reader)
      {
         m_name = reader.ReadString();
         int count = reader.ReadInt32();
         m_projects = new Dictionary<Guid, bool>(count);
         for (int i = 0; i < count; i++)
         {
            Guid id = reader.ReadGuid();
            bool included = reader.ReadBoolean();
            m_projects.Add(id, included);
         }
      }

      public ProjectSet(string name)
      {
         if (name == null)
            throw new ArgumentNullException("name", "name is null");

         m_projects = new Dictionary<Guid, bool>();
         m_name = name;
      }

      public string Name
      {
         get
         {
            return m_name;
         }

         set
         {
            SetValue(ref m_name, value);
         }
      }

      public bool? GetInclusionState(Guid projectId)
      {
         bool isIncluded;
         if (Projects.TryGetValue(projectId, out isIncluded))
            return isIncluded;
         else
            return null;
      }

      public void SetInclusionState(Guid projectId, bool? state)
      {
         if (state == null)
            m_projects.Remove(projectId);
         else
            m_projects[projectId] = state.Value;
      }

      public IReadOnlyDictionary<Guid, bool> Projects
      {
         get
         {
            return m_projects;
         }
      }

      public ISet<Guid> GetIncludedProjectIds(ISolutionHierarchyContainerItem solutionRoot)
      {
         if (solutionRoot == null)
            throw new ArgumentNullException("solutionRoot", "solutionRoot is null.");

         HashSet<Guid> ids = new HashSet<Guid>();

         return GetIncludedProjectIds(solutionRoot, ids, false);
      }

      public void PopulateFrom(ISolutionHierarchyContainerItem solutionRoot)
      {
         m_projects.Clear();
         foreach (var item in new StateItem(solutionRoot).GetAll().Where(s => s.State != null))
         {
            m_projects.Add(item.Item.Id, item.State.Value);
         }
      }


      private ISet<Guid> GetIncludedProjectIds(ISolutionHierarchyItem item, HashSet<Guid> ids, bool isParentIncluded)
      {
         bool value;
         bool? itemInclusionState;
         if (m_projects.TryGetValue(item.Id, out value))
         {
            itemInclusionState = value;
         }
         else
         {
            itemInclusionState = null;
         }

         bool isItemIncluded = isParentIncluded && itemInclusionState == null || itemInclusionState == true;
         if (isItemIncluded)
         {
            ids.Add(item.Id);
         }

         ISolutionHierarchyContainerItem container = item as ISolutionHierarchyContainerItem;
         if (container != null)
         {
            foreach (ISolutionHierarchyItem child in container.Children)
            {
               GetIncludedProjectIds(child, ids, isItemIncluded);
            }
         }

         return ids;
      }

      public void Serialize(BinaryWriter writer)
      {
         writer.Write(Name);
         writer.Write(m_projects.Count);
         foreach (var proj in m_projects)
         {
            writer.Write(proj.Key);
            writer.Write(proj.Value);
         }
      }

      private class StateItem
      {
         private List<StateItem> m_children;
         private ISolutionHierarchyItem m_item;

         public StateItem(ISolutionHierarchyItem item)
         {
            m_item = item;

            ISolutionHierarchyContainerItem container = item as ISolutionHierarchyContainerItem;
            if (container == null)
            {
               m_children = new List<StateItem>();
               State = item.ItemType != SolutionHierarchyItemType.UnloadedProject;
            }
            else
            {
               m_children = container.Children.Select(c => new StateItem(c)).ToList();

               int inclusionCount = m_children.Count(c => c.State == true);
               int exclusionCount = m_children.Count - inclusionCount;

               if (exclusionCount == 0)
               {
                  m_children.ForEach(si => si.State = null);
                  State = true;
               }
               else if (inclusionCount == 0)
               {
                  m_children.ForEach(si => si.State = null);
                  State = false;
               }
               else if (inclusionCount == exclusionCount && item.ItemType != SolutionHierarchyItemType.Solution || inclusionCount > exclusionCount)
               {
                  State = true;
                  m_children.ForEach(si => si.State = si.State == true ? (bool?)null : si.State);
               }
               else
               {
                  State = false;
                  m_children.ForEach(si => si.State = si.State == false ? (bool?)null : si.State);
               }
            }
         }

         public ISolutionHierarchyItem Item
         {
            get
            {
               return m_item;
            }
         }

         public bool? State { get; set; }

         public List<StateItem> Children
         {
            get
            {
               return m_children;
            }
         }

         public IEnumerable<StateItem> GetAll()
         {
            return Children.SelectMany(c => c.GetAll()).Concat(new[] { this });
         }
      }
   }
}
