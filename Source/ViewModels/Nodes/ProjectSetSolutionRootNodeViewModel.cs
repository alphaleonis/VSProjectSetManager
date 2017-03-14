using Alphaleonis.VSProjectSetMgr.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.VSProjectSetMgr.ViewModels.Nodes
{
   class ProjectSetSolutionRootNodeViewModel : ProjectSetContainerNodeViewModel
   {
      public ProjectSetSolutionRootNodeViewModel(ProjectSetViewModel owner, ISolutionHierarchyContainerItem node)
         : base(owner, node, null)
      {
      }
   }
}