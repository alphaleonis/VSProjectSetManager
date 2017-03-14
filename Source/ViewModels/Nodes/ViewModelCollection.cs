using Alphaleonis.VSProjectSetMgr.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Alphaleonis.VSProjectSetMgr.ViewModels.Nodes
{
   interface IModelWrapper<TModel>
   {
      TModel GetModel();
   }

   class ViewModelCollection<TViewModel, TModel> : ObservableBase, IReadOnlyList<TViewModel>, INotifyCollectionChanged, System.Collections.IList
      where TViewModel : class
      where TModel : class
   {
      #region Private Fields

      private List<TViewModel> m_viewModels;
      private IList<TModel> m_models;
      private ConditionalWeakTable<TModel, TViewModel> m_viewModelMap;
      private readonly Func<TModel, TViewModel> m_viewModelFactory;
      #endregion

      #region Events

      public event NotifyCollectionChangedEventHandler CollectionChanged;

      #endregion

      #region Construction

      public static ViewModelCollection<TViewModel, TModel> Create<TCollection>(TCollection modelCollection, Func<TModel, TViewModel> viewModelFactory)
         where TCollection : class, IList<TModel>, INotifyCollectionChanged
      {
         if (modelCollection == null)
            throw new ArgumentNullException("modelCollection", "modelCollection is null.");
         
         if (viewModelFactory == null)
            throw new ArgumentNullException("viewModelFactory", "viewModelFactory is null.");

         return new ViewModelCollection<TViewModel, TModel>(modelCollection, viewModelFactory);
      }

      private ViewModelCollection(IList<TModel> models, Func<TModel, TViewModel> viewModelFactory)
      {         
         CollectionChangedEventManager.AddHandler((INotifyCollectionChanged)models, SourceCollectionChanged);
         m_models = models;
         m_viewModelFactory = viewModelFactory;
         m_viewModelMap = new ConditionalWeakTable<TModel, TViewModel>();
         m_viewModels = new List<TViewModel>(models.Select(m => GetOrCreateVM(m)));
      }

      #endregion

      


      #region Properties

      public TViewModel this[int index]
      {
         get
         {
            return m_viewModels[index];
         }
      }

      public int Count
      {
         get
         {
            return m_viewModels.Count;
         }
      }

      #endregion

      #region Methods

      public IEnumerator<TViewModel> GetEnumerator()
      {
         return m_viewModels.GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      private void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         switch (e.Action)
         {
            case NotifyCollectionChangedAction.Add:
               ModelItemsAdded(e);
               break;
            case NotifyCollectionChangedAction.Remove:
               ModelItemsRemoved(e);
               break;
            case NotifyCollectionChangedAction.Replace:
               ModelItemsReplaced(e);
               break;
            case NotifyCollectionChangedAction.Move:
               ModelItemsMoved(e);
               break;
            case NotifyCollectionChangedAction.Reset:
               ModelItemsReset(e);
               break;
         }
      }

      private void ModelItemsReset(NotifyCollectionChangedEventArgs e)
      {
         m_viewModels.Clear();
         m_viewModels.AddRange(m_models.Select(m => GetOrCreateVM(m)));
         OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));         
      }

      private void ModelItemsMoved(NotifyCollectionChangedEventArgs e)
      {
         var viewModelsMoved = m_viewModels.GetRange(e.OldStartingIndex, e.OldItems.Count);
         m_viewModels.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
         m_viewModels.InsertRange(e.NewStartingIndex, viewModelsMoved);
         OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, viewModelsMoved, e.OldStartingIndex, e.NewStartingIndex));
      }

      private void ModelItemsReplaced(NotifyCollectionChangedEventArgs e)
      {
         List<TViewModel> viewModelsRemoved = new List<TViewModel>();
         foreach (TModel modelItemRemoved in e.OldItems.Cast<TModel>())
         {
            int index = m_viewModels.IndexOf(GetOrCreateVM(modelItemRemoved));
            if (index != -1)
            {
               viewModelsRemoved.Add(m_viewModels[index]);
               viewModelsRemoved.RemoveAt(index);
            }
         }

         IList<TViewModel> viewModelsAdded = e.NewItems.Cast<TModel>().Select(m => GetOrCreateVM(m)).ToArray();
         if (e.NewStartingIndex != -1)
            m_viewModels.InsertRange(e.NewStartingIndex, viewModelsAdded);
         else
            m_viewModels.AddRange(viewModelsAdded);

         OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, viewModelsRemoved, viewModelsAdded, e.NewStartingIndex));
      }

      private void ModelItemsRemoved(NotifyCollectionChangedEventArgs e)
      {
         List<TViewModel> removedItems = new List<TViewModel>();
         if (e.OldStartingIndex == -1)
         {
            foreach (var removedItem in e.OldItems.Cast<TModel>())
            {
               TViewModel viewModel = GetOrCreateVM(removedItem);
               int removalIndex = m_viewModels.IndexOf(viewModel);
               if (removalIndex != -1)
               {
                  m_viewModels.RemoveAt(removalIndex);
                  removedItems.Add(viewModel);
               }
            }
         }
         else
         {
            removedItems.AddRange(m_viewModels.Skip(e.OldStartingIndex).Take(e.OldItems.Count));
            m_viewModels.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
         }

         if (removedItems.Count > 0)
         {
            NotifyCollectionChangedEventArgs ne = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems);
            OnCollectionChanged(ne);
         }
      }

      private void ModelItemsAdded(NotifyCollectionChangedEventArgs e)
      {
         int index = e.NewStartingIndex;
         if (index == -1)
            index = Count;

         TViewModel[] list = e.NewItems.Cast<TModel>().Select(m => GetOrCreateVM(m)).ToArray();
         m_viewModels.InsertRange(e.NewStartingIndex, list);
         OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list, e.NewStartingIndex));
      }

      private TViewModel GetOrCreateVM(TModel modelItem)
      {
         return m_viewModelMap.GetValue(modelItem, (mi) => m_viewModelFactory(mi));
      }

      protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
      {
         NotifyCollectionChangedEventHandler handler = CollectionChanged;
         if (handler != null)
            handler(this, e);
      }
      #endregion

      int System.Collections.IList.Add(object value)
      {
         throw new NotSupportedException("Cannot modify a read-only collection.");
      }

      void System.Collections.IList.Clear()
      {
         throw new NotSupportedException("Cannot modify a read-only collection.");
      }

      bool System.Collections.IList.Contains(object value)
      {
         return m_viewModels.Contains(value as TViewModel);
      }

      int System.Collections.IList.IndexOf(object value)
      {
         return m_viewModels.IndexOf(value as TViewModel);
      }

      void System.Collections.IList.Insert(int index, object value)
      {
         throw new NotSupportedException("Cannot modify a read-only collection.");
      }

      bool System.Collections.IList.IsFixedSize
      {
         get 
         {
            return false;
         }
      }

      bool System.Collections.IList.IsReadOnly
      {
         get 
         {
            return true;
         }
      }

      void System.Collections.IList.Remove(object value)
      {
         throw new NotSupportedException("Cannot modify a read-only collection.");
      }

      void System.Collections.IList.RemoveAt(int index)
      {
         throw new NotSupportedException("Cannot modify a read-only collection.");
      }

      object System.Collections.IList.this[int index]
      {
         get
         {
            return m_viewModels[index];
         }
         set
         {
            throw new NotSupportedException("Cannot modify a read-only collection.");
         }
      }

      void System.Collections.ICollection.CopyTo(Array array, int index)
      {
         throw new NotSupportedException();
      }

      bool System.Collections.ICollection.IsSynchronized
      {
         get 
         {
            return false;
         }
      }

      object System.Collections.ICollection.SyncRoot
      {
         get 
         {
            return m_viewModels;
         }
      }
   }
}

