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
using Alphaleonis.VSProjectSetMgr.Views;
using Alphaleonis.VSProjectSetMgr.ViewModels.Nodes;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using System.Windows.Threading;

namespace Alphaleonis.VSProjectSetMgr
{
   public interface ISettingsProvider : System.IServiceProvider
   {
      ProjectSetManagerUserOptions GetSettings();
   }

   [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
   [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
   [ProvideMenuResource("Menus.ctmenu", 1)]
   [Guid(GuidList.guidLoadedProjectsProfileManagerPkgString)]
   [ProvideToolWindow(typeof(ProjectSetManagerToolWindow))]
   [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
   [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
   [ProvideService(typeof(SProjectSetRepository), IsAsyncQueryable = true)]
   [ProvideService(typeof(SInteractionService), IsAsyncQueryable = true)]
   [ProvideOptionPage(typeof(ProjectSetManagerUserOptions), "Project Set Manager", "General", 0, 0, true)]
   [ProvideProfileAttribute(typeof(ProjectSetManagerUserOptions), "Project Set Manager", "General", 0, 0, true)]
   public sealed class LoadedProjectsProfileManagerPackage : AsyncPackage, IVsSolutionEvents, ISettingsProvider
   {
      private uint m_eventsCookie;
      private const string OptionSolutionProfiles = "SolutionProfiles";
      private const int MRUSize = 4;
      private static object s_lock = new object();
      private bool m_isDisposed;

      #region Private Fields

      private ProjectSetRepository m_repository;
      private IInteractionService m_interactionService;

      #endregion

      public LoadedProjectsProfileManagerPackage()
      {
         Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
                  
      }

      #region Package Members

      /// <summary>
      /// Initialization of the package; this method is called right after the package is sited, so this is the place
      /// where you can put all the initialization code that rely on services provided by VisualStudio.
      /// </summary>
      protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
      {
         await base.InitializeAsync(cancellationToken, progress);
         AddOptionKey(OptionSolutionProfiles);

         m_interactionService = new InteractionService(this);
         AddService(typeof(SInteractionService), (s, c, t) => Task.FromResult((object)m_interactionService), true);
         m_repository = new ProjectSetRepository();
         AddService(typeof(SProjectSetRepository), (s, c, t) => Task.FromResult((object)m_repository), true);

         // Switches to the UI thread in order to consume some services used in command initialization
         await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

         // Add our command handlers for menu (commands must exist in the .vsct file)
         OleMenuCommandService mcs = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
         if (null != mcs)
         {
            for (int i = 0; i < MRUSize; i++)
            {
               mcs.AddCommand(new OleMenuCommand(null, null, OnMRUQueryStatus, GetCommandID(PkgCmdIDList.mnuidMRU0 + i)));
               mcs.AddCommand(new OleMenuCommand(OnExecuteMRULoadExclusive, GetCommandID(PkgCmdIDList.cmdidLoadExclusiveMRU0 + i)));
               mcs.AddCommand(new OleMenuCommand(OnExecuteMRULoad, GetCommandID(PkgCmdIDList.cmdidLoadMRU0 + i)));
               mcs.AddCommand(new OleMenuCommand(OnExecuteMRUUnload, GetCommandID(PkgCmdIDList.cmdidUnloadMRU0 + i)));
               mcs.AddCommand(new OleMenuCommand(OnExecuteMRUUnloadExclusive, GetCommandID(PkgCmdIDList.cmdidUnloadExclusiveMRU0 + i)));
            }

            mcs.AddCommand(new OleMenuCommand(OnExecuteUnloadAllProjectsInSolution, null, OnQueryStatusUnloadAllProjectsInSolution, GetCommandID(PkgCmdIDList.cmdidUnloadAllProjectsInSolution)));
            mcs.AddCommand(new OleMenuCommand(OnExecuteLoadAllProjectsInSolution, null, OnQueryStatusLoadAllProjectsInSolution, GetCommandID(PkgCmdIDList.cmdidLoadAllProjectsInSolution)));
            mcs.AddCommand(new MenuCommand(OnExecuteShowManagerToolWindow, GetCommandID(PkgCmdIDList.cmdidMore)));
            mcs.AddCommand(new MenuCommand(OnExecuteShowManagerToolWindow, GetCommandID(PkgCmdIDList.cmdidShowManager)));
         }

         AdviseSolutionEvents();
      }

      public void OnExecuteUnloadAllProjectsInSolution(object sender, EventArgs args)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
         {
            Debug.WriteLine(asm.FullName);
         }

         SolutionManager solMgr = GetSolutionManager();
         if (solMgr != null)
         {
            foreach (var project in solMgr.GetProjects(ProjectOptions.Loaded))
            {
               solMgr.UnloadProject(project);
            }
         }
      }

      public void OnQueryStatusUnloadAllProjectsInSolution(object sender, EventArgs args)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         OleMenuCommand command = sender as OleMenuCommand;
         if (command != null)
         {
            SolutionManager solMgr = GetSolutionManager();
            if (solMgr != null)
            {
               if (solMgr.GetProjects(ProjectOptions.Loaded).Any())
               {
                  command.Enabled = true;
                  return;
               }
            }
         }

         command.Enabled = false;
      }

      public void OnExecuteLoadAllProjectsInSolution(object sender, EventArgs args)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         SolutionManager solMgr = GetSolutionManager();
         if (solMgr != null)
         {
            foreach (var project in solMgr.GetProjects(ProjectOptions.Unloaded))
            {
               solMgr.LoadProject(project);
            }
         }
      }

      public void OnQueryStatusLoadAllProjectsInSolution(object sender, EventArgs args)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         OleMenuCommand command = sender as OleMenuCommand;
         if (command != null)
         {
            SolutionManager solMgr = GetSolutionManager();
            if (solMgr != null)
            {
               if (solMgr.GetProjects(ProjectOptions.Unloaded).Any())
               {
                  command.Enabled = true;
                  return;
               }
            }
         }

         command.Enabled = false;
      }

      private CommandID GetCommandID(int id)
      {
         return new CommandID(GuidList.guidLoadedProjectsProfileManagerCmdSet, id);
      }

      private void OnMRUQueryStatus(object sender, EventArgs e)
      {
         OleMenuCommand menu = sender as OleMenuCommand;
         if (menu != null)
         {
            var projectSet = GetMRUEntry(sender, PkgCmdIDList.mnuidMRU0);
            if (projectSet != null)
            {
               menu.Visible = true;
               menu.Text = projectSet.Name;
            }
            else
            {
               menu.Visible = false;
            }
         }
      }

      private void OnExecuteMRULoadExclusive(object sender, EventArgs e)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         var projectSet = GetMRUEntry(sender, PkgCmdIDList.cmdidLoadExclusiveMRU0);
         if (projectSet != null)
         {
            SolutionManager solMgr = GetSolutionManager();
            if (solMgr != null)
            {
               solMgr.LoadExclusive(projectSet.GetIncludedProjectIds(solMgr.GetSolutionHierarchy()));
            }
         }
      }

      private void OnExecuteMRULoad(object sender, EventArgs e)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         var projectSet = GetMRUEntry(sender, PkgCmdIDList.cmdidLoadMRU0);
         if (projectSet != null)
         {
            SolutionManager solMgr = GetSolutionManager();
            if (solMgr != null)
            {
               solMgr.Load(projectSet.GetIncludedProjectIds(solMgr.GetSolutionHierarchy()));
            }
         }
      }

      private void OnExecuteMRUUnload(object sender, EventArgs e)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         var projectSet = GetMRUEntry(sender, PkgCmdIDList.cmdidUnloadMRU0);
         if (projectSet != null)
         {
            SolutionManager solMgr = GetSolutionManager();
            if (solMgr != null)
            {
               solMgr.Unload(projectSet.GetIncludedProjectIds(solMgr.GetSolutionHierarchy()));
            }
         }
      }

      private void OnExecuteMRUUnloadExclusive(object sender, EventArgs e)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         var projectSet = GetMRUEntry(sender, PkgCmdIDList.cmdidUnloadExclusiveMRU0);
         if (projectSet != null)
         {
            SolutionManager solMgr = GetSolutionManager();
            if (solMgr != null)
            {
               solMgr.UnloadExclusive(projectSet.GetIncludedProjectIds(solMgr.GetSolutionHierarchy()));
            }
         }
      }

      private ProjectSet GetMRUEntry(object sender, int baseCommandId)
      {
         OleMenuCommand menu = sender as OleMenuCommand;
         if (menu != null)
         {
            int index = menu.CommandID.ID - baseCommandId;
            if (m_repository.ProjectSets.Count > index)
            {
               return m_repository.ProjectSets[index];
            }
         }

         return null;
      }

      private void OnExecuteShowManagerToolWindow(object sender, EventArgs e)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         // Get the instance number 0 of this tool window. This window is single instance so this instance
         // is actually the only one.
         // The last flag is set to true so that if the tool window does not exists it will be created.
         ToolWindowPane window = this.FindToolWindow(typeof(ProjectSetManagerToolWindow), 0, true);
         if ((null == window) || (null == window.Frame))
         {
            throw new NotSupportedException(Resources.CanNotCreateWindow);
         }
         IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
         Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
      }

      #endregion

      protected override void OnLoadOptions(string key, System.IO.Stream stream)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         if (key.Equals(OptionSolutionProfiles))
         {
            var settings = GetSettings();
            if (settings.Storage == ProjectSetProfileStorage.Solution)
            {
               try
               {
                  m_repository.LoadBinary(stream, GetSolutionManager());
               }
               catch (Exception ex)
               {
                  m_interactionService.ShowError("Error loading profile configuration from solution:\r\n{0}", ex.Message);
               }
            }
         }
         else
         {
            base.OnLoadOptions(key, stream);
         }
      }

      public ProjectSetManagerUserOptions GetSettings()
      {
         return (ProjectSetManagerUserOptions)GetDialogPage(typeof(ProjectSetManagerUserOptions));
      }

      protected override void OnSaveOptions(string key, System.IO.Stream stream)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         if (key.Equals(OptionSolutionProfiles))
         {
            var settings = GetSettings();

            if (settings.Storage == ProjectSetProfileStorage.ExternalFile)
            {
               try
               {
                  SaveOptionsExternal();
               }
               catch (Exception ex)
               {
                  m_interactionService.ShowError("Error saving profile configuration to external file:\r\n\r\n{1}", ex.Message);
               }
            }

            try
            {
               m_repository.SaveBinary(stream);
            }
            catch (Exception ex)
            {
               m_interactionService.ShowError("Error saving profile configuration from solution:\r\n\r\n{1}", ex.Message);
            }
         }
         else
         {
            base.OnSaveOptions(key, stream);
         }
      }

      private void SaveOptionsExternal()
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         SolutionManager solMgr = GetSolutionManager();
         if (solMgr != null)
         {
            SolutionInfo info = solMgr.GetSolutionInfo();
            IVsQueryEditQuerySave2 queryEditQuerySave = (IVsQueryEditQuerySave2)GetService(typeof(SVsQueryEditQuerySave));

            string targetPath = Path.ChangeExtension(info.SolutionFile, ".projectSets.json");
            if (m_repository.ProjectSets.Any() || File.Exists(targetPath))
            {

               string tempPath = Path.GetTempFileName();
               using (FileStream fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
               {
                  m_repository.SaveJson(fs);
               }

               if (!AreFilesIdentical(tempPath, targetPath))
               {
                  tagVSQuerySaveResult querySaveResult = tagVSQuerySaveResult.QSR_SaveOK;
                  if (queryEditQuerySave != null)
                  {
                     uint result;
                     ErrorHandler.ThrowOnFailure(queryEditQuerySave.QuerySaveFile(targetPath, 0, null, out result));
                     querySaveResult = (tagVSQuerySaveResult)result;
                  }

                  if (querySaveResult == tagVSQuerySaveResult.QSR_SaveOK)
                  {
                     var settings = GetSettings();
                     using (FileStream fin = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.None))
                     using (FileStream fout = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None))
                     {
                        fin.CopyTo(fout);
                     }
                  }
               }
            }
         }
      }

      private static bool AreFilesIdentical(string file1, string file2, int bufferSize = 2048)
      {
         if (!File.Exists(file1) || !File.Exists(file2))
            return false;

         byte[] buffer1 = new byte[bufferSize];
         byte[] buffer2 = new byte[bufferSize];

         using (FileStream fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read, FileShare.Read))
         using (FileStream fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read, FileShare.Read))
         {
            long totalBytes = 0;
            while (true)
            {
               if (fs1.Length != fs2.Length)
               {
                  return false;
               }

               int read1 = fs1.Read(buffer1, 0, buffer1.Length);
               int read2 = fs2.Read(buffer2, 0, buffer2.Length);

               if (read1 != read2)
                  return false;

               if (read1 > 0)
               {
                  for (int i = 0; i < read1; i++)
                  {
                     if (buffer1[i] != buffer2[i])
                        return false;
                  }
                  totalBytes += read1;
               }
               else
               {
                  return true;
               }
            }
         }
      }

      private void LoadOptionsExternal()
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         SolutionManager solMgr = GetSolutionManager();
         if (solMgr != null)
         {
            SolutionInfo info = solMgr.GetSolutionInfo();


            string targetBinaryPath = Path.ChangeExtension(info.SolutionFile, ".vspsm");
            string targetJsonPath = Path.ChangeExtension(info.SolutionFile, ".projectSets.json");

            if (File.Exists(targetJsonPath))
            {
               m_repository.LoadJson(targetJsonPath, solMgr);
            }
            else if (File.Exists(targetBinaryPath))
            {
               using (FileStream fs = new FileStream(targetBinaryPath, FileMode.Open, FileAccess.Read, FileShare.Read))
               {
                  m_repository.LoadBinary(fs, solMgr);
               }
            }
         }
      }

      private SolutionManager GetSolutionManager()
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         IVsSolution solution = (IVsSolution)GetService(typeof(SVsSolution));
         if (null != solution)
         {
            return new SolutionManager(solution, new OutputWindow(this));
         }

         return null;
      }

      private void AdviseSolutionEvents()
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         var solution = this.GetService(typeof(SVsSolution)) as IVsSolution;

         if (solution != null)
         {
            solution.AdviseSolutionEvents(this, out m_eventsCookie);
         }
      }

      #region IVsSolutionEvents

      public int OnAfterCloseSolution(object pUnkReserved)
      {
         m_repository.IsSolutionOpen = false;
         return VSConstants.S_OK;
      }

      public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         m_repository.IsSolutionOpen = true;

         var settings = GetSettings();
         if (settings.Storage == ProjectSetProfileStorage.ExternalFile)
         {
            try
            {
               LoadOptionsExternal();
            }
            catch (FileNotFoundException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (Exception ex)
            {
               m_interactionService.ShowError("Error loading profile configuration from external file:\r\n\r\n{1}", ex.Message);
            }
         }
         return VSConstants.S_OK;
      }


      public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
      {
         return VSConstants.E_NOTIMPL;
      }

      public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
      {
         return VSConstants.E_NOTIMPL;
      }

      public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
      {
         return VSConstants.E_NOTIMPL;
      }

      public int OnBeforeCloseSolution(object pUnkReserved)
      {
         return VSConstants.E_NOTIMPL;
      }

      public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
      {
         return VSConstants.E_NOTIMPL;
      }

      public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
      {
         return VSConstants.E_NOTIMPL;
      }

      public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
      {
         return VSConstants.E_NOTIMPL;
      }

      public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
      {
         return VSConstants.E_NOTIMPL;
      }

      #endregion

      protected override void Dispose(bool disposing)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();
         base.Dispose(disposing);
         if (!m_isDisposed)
         {
            lock (s_lock)
            {
               var solution = this.GetService(typeof(SVsSolution)) as IVsSolution;

               if (solution != null)
               {
                  if (disposing && m_eventsCookie != (uint)Microsoft.VisualStudio.Shell.Interop.Constants.VSCOOKIE_NIL)
                  {
                     ErrorHandler.ThrowOnFailure(solution.UnadviseSolutionEvents(m_eventsCookie));
                     m_eventsCookie = (uint)Microsoft.VisualStudio.Shell.Interop.Constants.VSCOOKIE_NIL;
                  }
                  m_isDisposed = true;
               }
            }
         }
      }
   }





}
