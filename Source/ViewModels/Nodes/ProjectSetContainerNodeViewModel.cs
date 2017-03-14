using Alphaleonis.VSProjectSetMgr.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.VSProjectSetMgr.ViewModels.Nodes
{
   abstract class ProjectSetContainerNodeViewModel : ProjectSetNodeViewModel
   {
      private List<ProjectSetNodeViewModel> m_children;

      protected ProjectSetContainerNodeViewModel(ProjectSetViewModel owner, ISolutionHierarchyContainerItem node, ProjectSetContainerNodeViewModel parent)
         : base(owner, node, parent)
      {
         m_children = new List<ProjectSetNodeViewModel>(node.Children.Count);
         foreach (var item in node.Children.OrderBy(c => c.ItemType == SolutionHierarchyItemType.SolutionFolder ? 0 : 1).ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase))
            m_children.Add(CreateFrom(owner, item, this));            
      }

      public IReadOnlyList<ProjectSetNodeViewModel> Children
      {
         get
         {
            return m_children;
         }
      }

      public override bool HasIncludedChildren
      {
         get
         {
            return Children.Any(c => c.IsIncluded == true || c.HasIncludedChildren);
         }
      }

      public override void OnParentStateChanged()
      {
         base.OnParentStateChanged();
         foreach (var child in Children)
            child.OnParentStateChanged();
      }

      public void SetExpandedRecursively(bool isExpanded)
      {
         foreach (var node in Children.OfType<ProjectSetContainerNodeViewModel>())
         {
            node.SetExpandedRecursively(isExpanded);
         }

         IsExpanded = isExpanded;
      }

      public override void UncheckRecursively()
      {
         foreach (var node in Children)
         {
            node.UncheckRecursively();
         }

         IsIncluded = null;
      }

      public override IEnumerable<ProjectSetNodeViewModel> GetDescendantNodesAndSelf()
      {
         foreach (var child in Children)
         {
            foreach (var node in child.GetDescendantNodesAndSelf())
               yield return node;
         }

         yield return this;
      }
      
   }
}

