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
   public interface INotifyBeforePropertyChanged
   {
      event EventHandler<BeforePropertyChangedEventArgs> BeforePropertyChanged;
   }
}
