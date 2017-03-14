using Alphaleonis.VSProjectSetMgr.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.VSProjectSetMgr.ViewModels.Nodes
{
   static class ProjectInfoStateExtensionMethods
   {
      public static bool IsIncluded(this InclusionExclusionCheckBoxState state)
      {
         return state == InclusionExclusionCheckBoxState.ImplicitlyIncluded || state == InclusionExclusionCheckBoxState.Included;
      }

      public static bool IsExplicit(this InclusionExclusionCheckBoxState state)
      {
         return state == InclusionExclusionCheckBoxState.Excluded || state == InclusionExclusionCheckBoxState.Included;
      }
   }
}

