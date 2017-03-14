using Alphaleonis.VSProjectSetMgr.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.VSProjectSetMgr.ViewModels.Nodes
{
   class ProjectSetSummaryViewModel : ObservableBase, IEditableObject
   {
      private readonly ProjectSet m_projectSet;
      private string m_editingName;
      private bool m_isEditing;

      public string Name
      {
         get
         {
            if (m_isEditing)
               return m_editingName;
            else
               return m_projectSet.Name;
         }

         set
         {
            if (m_isEditing)
            {
               SetValue(ref m_editingName, value);
            }
            else
            {
               if (m_projectSet.Name != value)
               {
                  m_projectSet.Name = value;
                  OnPropertyChanged();
               }
            }
         }
      }

      public ProjectSet ModelItem
      {
         get
         {
            return m_projectSet;
         }
      }
      public ProjectSetSummaryViewModel(ProjectSet projectSet)
      {
         if (projectSet == null)
            throw new ArgumentNullException("projectSet", "projectSet is null.");

         m_projectSet = projectSet;
      }

      public virtual void BeginEdit()
      {
         m_isEditing = true;
         m_editingName = m_projectSet.Name;
      }

      public virtual void CancelEdit()
      {
         m_isEditing = false;
         OnPropertyChanged("Name");
      }

      public virtual void EndEdit()
      {
         m_isEditing = false;
         m_projectSet.Name = m_editingName;
      }
   }

   class ProjectSetViewModel : ObservableBase, IEditableObject
   {
      #region Private Fields

      private ProjectSetSolutionRootNodeViewModel m_rootNode;
      private readonly ProjectSet m_projectSet;
      private string m_editingName;
      private Dictionary<Guid, bool?> m_savedState;

      #endregion

      #region Constructor

      public ProjectSetViewModel(ProjectSet projectSet, ISolutionHierarchyContainerItem solutionNode)
      {
         m_projectSet = projectSet;
         m_rootNode = new ProjectSetSolutionRootNodeViewModel(this, solutionNode);
      }

      #endregion


      #region Properties

      public string Name
      {
         get
         {
            if (m_savedState != null)
               return m_editingName;
            else
               return m_projectSet.Name;
         }

         set
         {
            if (m_savedState != null)
            {
               m_editingName = value;
               OnPropertyChanged();
            }
            else if (m_projectSet.Name != value)
            {
               m_projectSet.Name = value;
               OnPropertyChanged();
            }
         }
      }

      public ProjectSetSolutionRootNodeViewModel RootNode
      {
         get
         {
            return m_rootNode;
         }
      }

      public ProjectSet ModelItem
      {
         get
         {
            return m_projectSet;
         }
      }

      #endregion

      public void BeginEdit()
      {
         m_savedState = new Dictionary<Guid, bool?>();
         foreach (var node in RootNode.GetDescendantNodesAndSelf())
            m_savedState.Add(node.Id, node.IsIncluded);
         
         m_editingName = m_projectSet.Name;
      }

      public void CancelEdit()
      {
         foreach (var node in RootNode.GetDescendantNodesAndSelf())
            node.IsIncluded = m_savedState[node.Id];

         OnPropertyChanged("RootNode");
         OnPropertyChanged("Name");
         m_editingName = null;
         m_savedState = null;
      }

      public void EndEdit()
      {
         m_projectSet.Name = m_editingName;
         m_editingName = null;
         m_savedState = null;
      }
   }
}

