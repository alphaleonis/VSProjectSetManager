using Alphaleonis.VSProjectSetMgr.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Alphaleonis.VSProjectSetMgr.ViewModels.Nodes
{
   abstract class ProjectSetNodeViewModel : ObservableBase
   {
      #region Private Fields

      private readonly ISolutionHierarchyItem m_projectSetNode;
      private readonly ProjectSetContainerNodeViewModel m_parent;
      private bool m_isSelected;
      private bool m_isExpanded;      
      private readonly ProjectSetViewModel m_owner;

      #endregion

      #region Constructor

      public ProjectSetNodeViewModel(ProjectSetViewModel owner, ISolutionHierarchyItem projectSetNode, ProjectSetContainerNodeViewModel parent)
      {
         m_owner = owner;
         m_parent = parent;
         m_projectSetNode = projectSetNode;         
      }

      #endregion

      #region Properties

      public Guid Id
      {
         get
         {
            return m_projectSetNode.Id;
         }
      }

      public string Name
      {
         get
         {
            return m_projectSetNode.Name;
         }
      }

      public bool? IsIncluded
      {
         get
         {
            return m_owner.ModelItem.GetInclusionState(Id);
         }

         set
         {
            if (IsIncluded != value)
            {
               m_owner.ModelItem.SetInclusionState(Id, value);
               OnPropertyChanged();
               OnPropertyChanged("State");
               OnParentStateChanged();
               if (HasParent)
                  Parent.OnChildStateChanged();
            }
         }
      }

      public bool IsSelected
      {
         get
         {
            return m_isSelected;
         }

         set
         {
            SetValue(ref m_isSelected, value);
         }
      }

      public bool IsExpanded
      {
         get
         {
            return m_isExpanded;
         }

         set
         {
            SetValue(ref m_isExpanded, value);
         }
      }

      public ProjectSetContainerNodeViewModel Parent
      {
         get
         {
            return m_parent;
         }
      }

      public bool HasParent
      {
         get
         {
            return m_parent != null;
         }
      }

      public ImageSource Image
      {
         get
         {
            return Imaging.CreateBitmapSourceFromHIcon(m_projectSetNode.ImageHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
         }
      }

      public virtual InclusionExclusionCheckBoxState State
      {
         get
         {
            if (IsIncluded == true)
               return InclusionExclusionCheckBoxState.Included;
            else if (IsIncluded == false)
               return InclusionExclusionCheckBoxState.Excluded;
            else if (HasParent && Parent.State.IsIncluded())
               return InclusionExclusionCheckBoxState.ImplicitlyIncluded;
            else if (HasIncludedChildren)
               return InclusionExclusionCheckBoxState.PartiallyIncluded;
            else
               return InclusionExclusionCheckBoxState.Unchecked;
         }

         set
         {
            switch (value)
            {
               case InclusionExclusionCheckBoxState.Unchecked:
                  IsIncluded = null;
                  break;
               case InclusionExclusionCheckBoxState.ImplicitlyIncluded:
                  IsIncluded = null;
                  break;
               case InclusionExclusionCheckBoxState.Included:
                  IsIncluded = true;
                  break;
               case InclusionExclusionCheckBoxState.Excluded:
                  IsIncluded = false;
                  break;
               case InclusionExclusionCheckBoxState.PartiallyIncluded:
                  IsIncluded = null;
                  break;
            }
         }
      }

      #endregion

      #region Methods

      public virtual bool HasIncludedChildren
      {
         get
         {
            return false;
         }
      }

      public virtual void OnParentStateChanged()
      {
         if (!IsIncluded.HasValue)
            OnPropertyChanged("State");
      }

      public virtual void OnChildStateChanged()
      {
         if (!IsIncluded.HasValue)
         {
            OnPropertyChanged("State");
            if (Parent != null)
               Parent.OnChildStateChanged();
         }         
      }

      public abstract IEnumerable<ProjectSetNodeViewModel> GetDescendantNodesAndSelf();

      public static ProjectSetNodeViewModel CreateFrom(ProjectSetViewModel owner, ISolutionHierarchyItem node, ProjectSetContainerNodeViewModel parent)
      {
         if (node == null)
            throw new ArgumentNullException("node", "node is null.");

         
         if (parent == null && node.ItemType != SolutionHierarchyItemType.Solution)
            throw new ArgumentNullException("parent", "parent is null.");

         switch (node.ItemType)
         {
            case SolutionHierarchyItemType.Solution:
               return new ProjectSetSolutionRootNodeViewModel(owner, (ISolutionHierarchyContainerItem)node);

            case SolutionHierarchyItemType.SolutionFolder:
               return new ProjectSetSolutionFolderNodeViewModel(owner, (ISolutionHierarchyContainerItem)node, parent);

            case SolutionHierarchyItemType.Project:
            case SolutionHierarchyItemType.UnloadedProject:
               return new ProjectSetProjectNodeViewModel(owner, node, parent);

            default:
               throw new NotSupportedException(String.Format("Unknown project set node type {0}", node.ItemType));
         }
      }

      public virtual void UncheckRecursively()
      {
         IsIncluded = null;
      }

      #endregion
   }
}