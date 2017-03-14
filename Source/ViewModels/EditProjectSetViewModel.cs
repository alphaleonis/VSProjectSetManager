using Alphaleonis.VSProjectSetMgr.Controls;
using Alphaleonis.VSProjectSetMgr.ViewModels.Nodes;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace Alphaleonis.VSProjectSetMgr.Views
{
   class EditProjectSetViewModel : ObservableBase
   {
      #region Private Fields

      private ProjectSetNodeViewModel m_selectedProject;
      private IServiceProvider m_services;
      private ProjectSetViewModel m_projectSet;
      private string m_name;
      private readonly DelegateCommand m_okCommand;
      private readonly DelegateCommand m_cancelCommand;
      private readonly DelegateCommand m_expandSubtreeCommand;
      private readonly DelegateCommand m_collapseAllCommand;
      private readonly DelegateCommand m_uncheckAllCommand;

      #endregion

      #region Events

      public event EventHandler<CloseDialogEventArgs> CloseDialog;

      #endregion

      #region Constructor

      public EditProjectSetViewModel(IServiceProvider services, ProjectSetViewModel projectSet)
      {         
         m_services = services;
         m_projectSet = projectSet;
         m_name = m_projectSet.Name;
         m_okCommand = new DelegateCommand(ExecuteOk);
         m_cancelCommand = new DelegateCommand(ExecuteCancel);
         m_expandSubtreeCommand = new DelegateCommand(ExecuteExpandSubtree, CanExecuteExpandSubtree);
         m_collapseAllCommand = new DelegateCommand(CollapseAll);
         m_uncheckAllCommand = new DelegateCommand(UncheckAll);
         projectSet.RootNode.IsExpanded = true;
      }

      #endregion

      #region Properties
      
      public string Name
      {
         get
         {
            return m_projectSet.Name;
         }

         set
         {
            if (m_name != value)
            {
               m_projectSet.Name = value;
               OnPropertyChanged();
            }
         }
      }

      public DelegateCommand ExpandSubtreeCommand
      {
         get
         {
            return m_expandSubtreeCommand;
         }
      }

      public DelegateCommand OkCommand
      {
         get
         {
            return m_okCommand;
         }
      }

      public DelegateCommand CancelCommand
      {
         get
         {
            return m_cancelCommand;
         }
      }

      public DelegateCommand CollapseAllCommand
      {
         get
         {
            return m_collapseAllCommand;
         }
      }

      public DelegateCommand UncheckAllCommand
      {
         get
         {
            return m_uncheckAllCommand;
         }
      }

      public ProjectSetSolutionRootNodeViewModel RootNode
      {
         get
         {
            return m_projectSet.RootNode;
         }
      }

      public ProjectSetNodeViewModel SelectedProject
      {
         get
         {
            return m_selectedProject;
         }

         set
         {
            SetValue(ref m_selectedProject, value);
            ExpandSubtreeCommand.RaiseCanExecuteChanged();
         }
      }
    
      #endregion


      #region Methods

      private void ExecuteOk()
      {
         OnCloseDialog(this, new CloseDialogEventArgs(true));
      }

      private void ExecuteCancel()
      {
         OnCloseDialog(this, new CloseDialogEventArgs(false));
      }

      private void ExecuteExpandSubtree()
      {         
         if (m_selectedProject != null)
         {
            ProjectSetContainerNodeViewModel folder = m_selectedProject as ProjectSetContainerNodeViewModel;
            if (folder != null)
            {
               folder.SetExpandedRecursively(true);
            }
         }
      }

      private bool CanExecuteExpandSubtree()
      {
         return m_selectedProject != null && m_selectedProject is ProjectSetContainerNodeViewModel;
      }

      private void CollapseAll()
      {
         CollapseAll(m_projectSet.RootNode);
      }

      private void CollapseAll(ProjectSetNodeViewModel node)
      {
         ProjectSetContainerNodeViewModel containerNode = node as ProjectSetContainerNodeViewModel;
         if (containerNode != null)
         {
            foreach (var child in containerNode.Children)
               CollapseAll(child);

            containerNode.IsExpanded = false;
         }
      }

      private void UncheckAll()
      {         
         if (m_selectedProject != null)
         {
            ProjectSetContainerNodeViewModel folder = m_selectedProject as ProjectSetContainerNodeViewModel;
            if (folder != null)
            {
               folder.UncheckRecursively();
            }
            else
            {
               m_selectedProject.IsIncluded = null;
            }
         }
      }

      protected virtual void OnCloseDialog(object sender, CloseDialogEventArgs e)
      {
         EventHandler<CloseDialogEventArgs> handler = CloseDialog;
         if (handler != null)
            handler(sender, e);
      }

      #endregion
   }

}