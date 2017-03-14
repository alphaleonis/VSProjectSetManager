using Alphaleonis.VSProjectSetMgr.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.VSProjectSetMgr.ViewModels.Nodes
{
   class ProjectSetProjectNodeViewModel : ProjectSetNodeViewModel
   {
      public ProjectSetProjectNodeViewModel(ProjectSetViewModel owner, ISolutionHierarchyItem node, ProjectSetContainerNodeViewModel parent)
         : base(owner, node, parent)
      {
      }

      public override IEnumerable<ProjectSetNodeViewModel> GetDescendantNodesAndSelf()
      {
         yield return this;
      }
   }
}

