using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Alphaleonis.VSProjectSetMgr
{
   public class ObservableBase : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;


      protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
      {
         Debug.Assert(this.GetType().GetProperty(e.PropertyName) != null, String.Format("No property with the specified name \"{0}\" exists in this class ({1}).", e.PropertyName, GetType().Name));
         PropertyChangedEventHandler handler = PropertyChanged;
         if (handler != null)
            handler(this, e);
      }

      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
      {
         OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
      }

      protected virtual bool SetValue<T>(ref T backingField, T newValue, [CallerMemberName] string propertyName = "") 
      {
         if (!EqualityComparer<T>.Default.Equals(backingField, newValue))
         {
            backingField = newValue;
            OnPropertyChanged(propertyName);
            return true;
         }
         
         return false;
      }
   }
}