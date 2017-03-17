using Alphaleonis.VSProjectSetMgr.ViewModels.Nodes;
using Alphaleonis.VSProjectSetMgr.Views;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Alphaleonis.VSProjectSetMgr
{
   sealed class ProjectSetManagerToolWindowViewModel : ObservableBase
   {
      private ProjectSetSummaryViewModel m_selectedItem;
      private readonly IServiceProvider m_serviceProvider;
      private readonly IProjectSetRepository m_repository;
      private readonly ProjectSetRepositoryViewModel m_repositoryViewModel;
      private readonly ICollectionView m_collectionView;
      private readonly IInteractionService m_interactionService;
      
      private readonly DelegateCommand m_loadCommand;
      private readonly DelegateCommand m_loadExCommand;
      private readonly DelegateCommand m_unloadCommand;
      private readonly DelegateCommand m_unloadExCommand;
      private readonly DelegateCommand m_addCommand;
      private readonly DelegateCommand m_editCommand;
      private readonly DelegateCommand m_deleteCommand;

      public ProjectSetManagerToolWindowViewModel(IServiceProvider serviceProvider)
      {       
         m_serviceProvider = serviceProvider;
         m_repository = (IProjectSetRepository)m_serviceProvider.RequireService<SProjectSetRepository>();
         m_interactionService = (IInteractionService)m_serviceProvider.RequireService<SInteractionService>();

         if (m_repository != null)
         {
            m_repositoryViewModel = new ProjectSetRepositoryViewModel(m_repository);
            m_collectionView = new ListCollectionView(m_repositoryViewModel.ProjectSets);
            //m_collectionView = CollectionViewSource.GetDefaultView(m_repository.ProjectSets);
            m_collectionView.SortDescriptions.Clear();
            m_collectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
         }

         OleMenuCommandService mcs = m_serviceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
         if (null != mcs)
         {
            mcs.AddCommand(new OleMenuCommand(OnDeleteProfile, null, OnQuerySelectedItemCommandStatus, GetCommandID(PkgCmdIDList.cmdidDeleteProfile)));
            mcs.AddCommand(new OleMenuCommand(OnAddProfile, null, OnQueryGlobalCommandStatus, GetCommandID(PkgCmdIDList.cmdidAddProfile)));
            mcs.AddCommand(new OleMenuCommand(OnEditProfile, null, OnQuerySelectedItemCommandStatus, GetCommandID(PkgCmdIDList.cmdidEditProfile)));
            mcs.AddCommand(new OleMenuCommand(OnLoadProfile, null, OnQuerySelectedItemCommandStatus, GetCommandID(PkgCmdIDList.cmdidLoadSelectedProfile)));
            mcs.AddCommand(new OleMenuCommand(OnUnloadProfile, null, OnQuerySelectedItemCommandStatus, GetCommandID(PkgCmdIDList.cmdidUnloadSelectedProfile)));
            mcs.AddCommand(new OleMenuCommand(OnLoadExProfile, null, OnQuerySelectedItemCommandStatus, GetCommandID(PkgCmdIDList.cmdidLoadExSelectedProfile)));
            mcs.AddCommand(new OleMenuCommand(OnUnloadExProfile, null, OnQuerySelectedItemCommandStatus, GetCommandID(PkgCmdIDList.cmdidUnloadExSelectedProfile)));
         }

         m_loadCommand = new DelegateCommand(() => OnLoadProfile(this, EventArgs.Empty), CanExecuteSelectedItemCommand);
         m_loadExCommand = new DelegateCommand(() => OnLoadExProfile(this, EventArgs.Empty), CanExecuteSelectedItemCommand);
         m_unloadCommand = new DelegateCommand(() => OnUnloadProfile(this, EventArgs.Empty), CanExecuteSelectedItemCommand);
         m_unloadExCommand = new DelegateCommand(() => OnUnloadExProfile(this, EventArgs.Empty), CanExecuteSelectedItemCommand);
         m_addCommand = new DelegateCommand(() => OnAddProfile(this, EventArgs.Empty));
         m_editCommand = new DelegateCommand(() => OnEditProfile(this, EventArgs.Empty), CanExecuteSelectedItemCommand);
         m_deleteCommand = new DelegateCommand(() => OnDeleteProfile(this, EventArgs.Empty), CanExecuteSelectedItemCommand);         
      }

      #region Properties

      public ICommand AddCommand
      {
         get
         {
            return m_addCommand;
         }
      }
      public ICommand DeleteCommand
      {
         get
         {
            return m_deleteCommand;
         }
      }
      public ICommand EditCommand
      {
         get
         {
            return m_editCommand;
         }
      }

      public ICommand LoadCommand
      {
         get
         {
            return m_loadCommand;
         }
      }

      public ICommand LoadExCommand
      {
         get
         {
            return m_loadExCommand;
         }
      }
      public ICollectionView ProjectSets
      {
         get
         {
            return m_collectionView;
         }
      }

      public ICommand UnloadCommand
      {
         get
         {
            return m_unloadCommand;
         }
      }

      public ICommand UnloadExCommand
      {
         get
         {
            return m_unloadExCommand;
         }
      }
      public ProjectSetSummaryViewModel SelectedItem
      {
         get
         {
            return m_selectedItem;
         }

         set
         {
            bool wasNull = m_selectedItem == null;
            if (SetValue(ref m_selectedItem, value))
            {
               UpdateUI(wasNull);
               m_editCommand.RaiseCanExecuteChanged();
               m_deleteCommand.RaiseCanExecuteChanged();
               m_loadCommand.RaiseCanExecuteChanged();
               m_unloadCommand.RaiseCanExecuteChanged();
               m_loadExCommand.RaiseCanExecuteChanged();
               m_unloadExCommand.RaiseCanExecuteChanged();
            }
         }
      }

      #endregion

      private void OnDeleteProfile(object sender, EventArgs e)
      {
         if (SelectedItem != null)
         {
            if (m_interactionService.ShowDialog("Delete Project Set", String.Format("Are you sure you want to delete the project set \"{0}\" from this solution?\r\nNote: This does not remove the actual projects from the solution, only the configuration used for loading/unloading them.", SelectedItem.Name), OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND, OLEMSGICON.OLEMSGICON_QUERY) == Microsoft.VisualStudio.VSConstants.MessageBoxResult.IDYES)
               m_repository.ProjectSets.Remove(SelectedItem.ModelItem);
         }
      }

      private void OnAddProfile(object sender, EventArgs e)
      {
         SolutionManager solMgr = GetSolutionManager();
         if (solMgr != null)
         {
            var solutionHierarchy = solMgr.GetSolutionHierarchy();
            ProjectSet projectSet = new ProjectSet("New Project Set");
            projectSet.PopulateFrom(solutionHierarchy);
            ProjectSetViewModel projectSetVm = new ProjectSetViewModel(projectSet, solutionHierarchy);
            bool? result = EditProjectSetDialog.ShowDialog(m_serviceProvider, projectSetVm, "Create Project Set", () =>
               {
                  if (m_repositoryViewModel.ProjectSets.Any(ps => ps.Name.Equals(projectSet.Name, StringComparison.OrdinalIgnoreCase)))
                  {
                     Microsoft.VisualStudio.VSConstants.MessageBoxResult overwriteResponse = m_interactionService.ShowDialog("Overwrite Project Set", String.Format("A project set with the name \"{0}\" already exists in this solution. Do you want to overwrite it?", projectSet.Name), OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND, OLEMSGICON.OLEMSGICON_QUERY);
                     if (overwriteResponse == Microsoft.VisualStudio.VSConstants.MessageBoxResult.IDNO)
                     {
                        return false;
                     }
                  }
                  return true;
               });

            if (result == true)
            {
               int replacementIndex = -1;
               for (int i = 0; i < m_repository.ProjectSets.Count; i++)
               {
                  if (m_repository.ProjectSets[i].Name.Equals(projectSet.Name, StringComparison.OrdinalIgnoreCase))
                  {
                     replacementIndex = i;
                     break;
                  }
               }

               if (replacementIndex != -1)
               {
                  m_repository.ProjectSets[replacementIndex] = projectSet;
               }
               else
               {
                  m_repository.ProjectSets.Add(projectSet);
               }
            }
         }

      }

      private void OnEditProfile(object sender, EventArgs e)
      {
         SolutionManager solMgr = GetSolutionManager();

         if (solMgr != null)
         {
         ProjectSetSummaryViewModel item = SelectedItem;
         if (item != null)
         {
            ProjectSetViewModel projSetVm = new ProjectSetViewModel(item.ModelItem, solMgr.GetSolutionHierarchy());
            projSetVm.BeginEdit();
            try
            {
               bool? result = EditProjectSetDialog.ShowDialog(m_serviceProvider, projSetVm, "Edit Project Set", () =>
                  {
                     if (m_repositoryViewModel.ProjectSets.Any(ps => ps.Name.Equals(projSetVm.Name, StringComparison.OrdinalIgnoreCase) && !object.ReferenceEquals(ps.ModelItem, projSetVm.ModelItem)))
                     {
                        m_interactionService.ShowDialog("Name Conflict", String.Format("Another project set already has the name \"{0}\". Please select another name.", projSetVm.Name), OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_WARNING);
                        return false;
                     }

                     return true;
                  });

               if (result == true)
               {
                  projSetVm.EndEdit();
                  m_repository.ProjectSets.Remove(projSetVm.ModelItem);
                  m_repository.ProjectSets.Add(projSetVm.ModelItem);
                  SelectedItem = item;
               }
               else
               {
                  projSetVm.CancelEdit();
                  m_repository.ProjectSets.Remove(projSetVm.ModelItem);
                  m_repository.ProjectSets.Add(projSetVm.ModelItem);
                  SelectedItem = item;
               }
            }
            catch
            {
               projSetVm.CancelEdit();
            }
         }
         }
      }

      private SolutionManager GetSolutionManager()
      {
         IVsSolution solution = (IVsSolution)m_serviceProvider.GetService(typeof(SVsSolution));
         if (null != solution)
         {
            OutputWindow outputWindow = new OutputWindow(m_serviceProvider);
            return new SolutionManager(solution, outputWindow);
         }

         return null;
      }

      private void OnLoadProfile(object sender, EventArgs e)
      {
         SolutionManager solMgr = GetSolutionManager();
         if (solMgr != null)
         {
            if (SelectedItem != null)
            {
               solMgr.Load(SelectedItem.ModelItem.GetIncludedProjectIds(solMgr.GetSolutionHierarchy()));
            }
         }

      }

      private void OnUnloadProfile(object sender, EventArgs e)
      {
         SolutionManager solMgr = GetSolutionManager();
         if (solMgr != null)
         {
            if (SelectedItem != null)
            {
               solMgr.Unload(SelectedItem.ModelItem.GetIncludedProjectIds(solMgr.GetSolutionHierarchy()));
            }
         }
      }

      private void OnLoadExProfile(object sender, EventArgs e)
      {
         SolutionManager solMgr = GetSolutionManager();
         if (solMgr != null)
         {
            if (SelectedItem != null)
            {
               solMgr.LoadExclusive(SelectedItem.ModelItem.GetIncludedProjectIds(solMgr.GetSolutionHierarchy()));
            }
         }
      }

      private void OnUnloadExProfile(object sender, EventArgs e)
      {
         SolutionManager solMgr = GetSolutionManager();
         if (solMgr != null)
         {
            if (SelectedItem != null)
            {
               solMgr.UnloadExclusive(SelectedItem.ModelItem.GetIncludedProjectIds(solMgr.GetSolutionHierarchy()));
            }
         }
      }

      private void OnQuerySelectedItemCommandStatus(object sender, EventArgs e)
      {
         OleMenuCommand command = sender as OleMenuCommand;
         if (command != null)
         {
            command.Enabled = m_repository.IsSolutionOpen && SelectedItem != null;
         }
      }

      private bool CanExecuteSelectedItemCommand()
      {
         return m_repository.IsSolutionOpen && SelectedItem != null;         
      }

      private void OnQueryGlobalCommandStatus(object sender, EventArgs e)
      {
         OleMenuCommand command = sender as OleMenuCommand;
         if (command != null)
         {
            command.Enabled = m_repository.IsSolutionOpen;
         }
      }
      private CommandID GetCommandID(int id)
      {
         return new CommandID(GuidList.guidLoadedProjectsProfileManagerCmdSet, id);
      }

      void UpdateUI(bool synchronous)
      {
         IVsUIShell vsShell = (IVsUIShell)m_serviceProvider.GetService(typeof(IVsUIShell));
         if (vsShell != null)
         {
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(vsShell.UpdateCommandUI(synchronous ? 1 : 0));
         }
      }
   }
}