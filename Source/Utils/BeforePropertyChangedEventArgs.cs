using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.VSProjectSetMgr
{
   public class BeforePropertyChangedEventArgs : PropertyChangedEventArgs
   {
      private readonly object m_oldValue;
      private readonly object m_newValue;

      public BeforePropertyChangedEventArgs(string propertyName, object oldValue, object newValue)
         : base(propertyName)
      {
         m_oldValue = oldValue;
         m_newValue = newValue;
      }

      public object NewValue
      {
         get
         {
            return m_newValue;
         }
      }

      public object OldValue
      {
         get
         {
            return m_oldValue;
         }
      }
   }
}
