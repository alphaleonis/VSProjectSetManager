using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.VSProjectSetMgr
{
   [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Disposal of the monitor is actually not needed")]
   public abstract class ObservableKeyedCollection<TKey, TItem> : KeyedCollection<TKey, TItem>, INotifyCollectionChanged
   {
      #region Private Fields
      
      private readonly SimpleMonitor m_monitor;

      #endregion

      #region INotifyCollectionChanged Members

      public event NotifyCollectionChangedEventHandler CollectionChanged;

      #endregion

      #region Constructor

      public ObservableKeyedCollection()
         : base()
      {
         m_monitor = new SimpleMonitor();
      }

      public ObservableKeyedCollection(IEqualityComparer<TKey> comparer)
         : base(comparer)
      {
         m_monitor = new SimpleMonitor();
      }

      #endregion

      #region Methods

      protected override void SetItem(int index, TItem item)
      {
         CheckReentrancy();
         base.SetItem(index, item);
         OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, index));
      }

      protected override void InsertItem(int index, TItem item)
      {
         CheckReentrancy();
         base.InsertItem(index, item);
         OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
      }

      protected override void ClearItems()
      {
         CheckReentrancy();
         base.ClearItems();
         OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
      }

      protected override void RemoveItem(int index)
      {
         CheckReentrancy();
         TItem item = this[index];
         base.RemoveItem(index);
         OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
      }

      protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
      {
         var handler = CollectionChanged;
         if (handler != null)
         {
            using (BlockReentrancy())
            {
               handler(this, e);
            }
         }
      }

      protected IDisposable BlockReentrancy()
      {
         this.m_monitor.Enter();
         return this.m_monitor;
      }

      protected void CheckReentrancy()
      {
         if ((this.m_monitor.Busy && (CollectionChanged != null)) && (CollectionChanged.GetInvocationList().Length > 1))
         {
            throw new InvalidOperationException("Collection Reentrancy Not Allowed");
         }
      }

      #endregion

      #region Nested Types

      [Serializable]
      private class SimpleMonitor : IDisposable
      {
         private int m_busyCount;

         public bool Busy
         {
            get { return this.m_busyCount > 0; }
         }

         public void Enter()
         {
            this.m_busyCount++;
         }

         public void Dispose()
         {
            this.m_busyCount--;
         }
      }

      #endregion
   }
}