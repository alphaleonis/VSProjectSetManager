using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alphaleonis.VSProjectSetMgr
{
   public class ProgressInfo : IProgressInfo
   {
      private readonly int m_percentComplete;
      private readonly string m_currentOperation;
      
      public ProgressInfo(int percentComplete, string currentOperation)
      {
         m_percentComplete = percentComplete;
         m_currentOperation = currentOperation;
      }

      public string CurrentOperation
      {
         get
         {
            return m_currentOperation;
         }
      }

      public int PercentComplete
      {
         get
         {
            return m_percentComplete;
         }
      }
      
      
   }
}
