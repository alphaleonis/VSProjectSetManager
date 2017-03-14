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
using System.Windows;
using System.Windows.Input;

namespace Alphaleonis.VSProjectSetMgr
{
   public abstract class DelegateCommandBase : ICommand
   {
      #region Private Fields

      private readonly Func<object, bool> m_canExecuteMethod;
      private readonly Action<object> m_executeMethod;

      #endregion

      #region Events

      public event EventHandler CanExecuteChanged
      {
         add
         {
            DelegateCommandWeakEventManager.AddHandler(this, value);
         }
         remove
         {
            DelegateCommandWeakEventManager.RemoveHandler(this, value);
         }
      }

      #endregion

      #region Constructor

      protected DelegateCommandBase(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
      {
         if (executeMethod == null)
            throw new ArgumentNullException("executeMethod", "executeMethod is null.");

         this.m_executeMethod = executeMethod;
         this.m_canExecuteMethod = canExecuteMethod;
      }

      #endregion

      #region Methods

      protected bool CanExecute(object parameter)
      {
         if (m_canExecuteMethod != null)
         {
            return m_canExecuteMethod(parameter);
         }
         return true;
      }

      protected void Execute(object parameter)
      {
         m_executeMethod(parameter);
      }

      protected virtual void OnCanExecuteChanged()
      {
         DelegateCommandWeakEventManager.OnCanExecuteChanged(this, EventArgs.Empty);
      }

      public void RaiseCanExecuteChanged()
      {
         OnCanExecuteChanged();
      }

      bool ICommand.CanExecute(object parameter)
      {
         return CanExecute(parameter);
      }

      void ICommand.Execute(object parameter)
      {
         Execute(parameter);
      }

      #endregion

      #region Nested Types

      class DelegateCommandWeakEventManager : WeakEventManager
      {

         /// <summary>
         /// Add a handler for the given source's event.
         /// </summary>
         public static void AddHandler(object source, EventHandler handler)
         {
            if (source == null)
               throw new ArgumentNullException("source");

            if (handler == null)
               throw new ArgumentNullException("handler");

            CurrentManager.ProtectedAddHandler(source, handler);
         }

         /// <summary>
         /// Remove a handler for the given source's event.
         /// </summary>
         public static void RemoveHandler(object source, EventHandler handler)
         {
            if (source == null)
               throw new ArgumentNullException("source");

            if (handler == null)
               throw new ArgumentNullException("handler");

            CurrentManager.ProtectedRemoveHandler(source, handler);
         }

         /// <summary>
         /// Get the event manager for the current thread.
         /// </summary>
         private static DelegateCommandWeakEventManager CurrentManager
         {
            get
            {
               Type managerType = typeof(DelegateCommandWeakEventManager);
               DelegateCommandWeakEventManager manager = (DelegateCommandWeakEventManager)GetCurrentManager(managerType);

               // at first use, create and register a new manager
               if (manager == null)
               {
                  manager = new DelegateCommandWeakEventManager();
                  SetCurrentManager(managerType, manager);
               }

               return manager;
            }
         }



         /// <summary>
         /// Return a new list to hold listeners to the event.
         /// </summary>
         protected override ListenerList NewListenerList()
         {
            return new ListenerList();
         }


         /// <summary>
         /// Listen to the given source for the event.
         /// </summary>
         protected override void StartListening(object source)
         {
         }

         /// <summary>
         /// Stop listening to the given source for the event.
         /// </summary>
         protected override void StopListening(object source)
         {
         }

         /// <summary>
         /// Event handler for the SomeEvent event.
         /// </summary>
         public static void OnCanExecuteChanged(object sender, EventArgs e)
         {
            CurrentManager.DeliverEvent(sender, e);
         }
      }

      #endregion
   }

   public class DelegateCommand : DelegateCommandBase
   {
      public DelegateCommand(Action executeMethod)
         : this(executeMethod, () => true)
      {
      }

      public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
         : base(executeMethod == null ? (Action<object>)null : o => executeMethod(), canExecuteMethod == null ? (Func<object, bool>)null : o => canExecuteMethod())
      {
      }

      public bool CanExecute()
      {
         return base.CanExecute(null);
      }

      public void Execute()
      {
         base.Execute(null);
      }
   }

   public class DelegateCommand<T> : DelegateCommandBase
   {
      public DelegateCommand(Action<T> executeMethod)
         : this(executeMethod, null)
      {
      }

      public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
         : base(executeMethod == null ? (Action<object>)null : o => executeMethod((T)o), canExecuteMethod == null ? (Func<object, bool>)null : o => canExecuteMethod((T)o))
      {
      }

      public bool CanExecute()
      {
         return base.CanExecute(null);
      }

      public void Execute()
      {
         base.Execute(null);
      }
   }
}