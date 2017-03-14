using Alphaleonis.VSProjectSetMgr.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.VSProjectSetMgr.ViewModels.Nodes
{
   class ProjectSetSolutionFolderNodeViewModel : ProjectSetContainerNodeViewModel
   {
      public ProjectSetSolutionFolderNodeViewModel(ProjectSetViewModel owner, ISolutionHierarchyContainerItem node, ProjectSetContainerNodeViewModel parent)
         : base(owner, node, parent)
      {         
      }
   }
}