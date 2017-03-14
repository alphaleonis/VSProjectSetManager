using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.VSProjectSetMgr
{
   public class CloseDialogEventArgs : EventArgs
   {
      private readonly bool? m_result;

      public CloseDialogEventArgs(bool? result)
      {
         m_result = result;
      }

      public bool? Result
      {
         get
         {
            return m_result;
         }
      }
   }
}
