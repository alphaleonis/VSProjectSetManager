using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Alphaleonis.VSProjectSetMgr.Controls
{
   /// <summary>
   /// Exposes attached behaviors that can be
   /// applied to TreeViewItem objects.
   /// </summary>
   public static class TreeViewItemBehavior
   {
      // The TreeViewItem that the mouse is currently directly over (or null).
      private static TreeViewItem s_currentItem;

      #region IsBroughtIntoViewWhenSelected

      public static bool GetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem)
      {
         return (bool)treeViewItem.GetValue(IsBroughtIntoViewWhenSelectedProperty);
      }

      public static void SetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem, bool value)
      {
         treeViewItem.SetValue(IsBroughtIntoViewWhenSelectedProperty, value);
      }

      public static readonly DependencyProperty IsBroughtIntoViewWhenSelectedProperty = DependencyProperty.RegisterAttached("IsBroughtIntoViewWhenSelected",
          typeof(bool), typeof(TreeViewItemBehavior), new UIPropertyMetadata(false, OnIsBroughtIntoViewWhenSelectedChanged));

      static void OnIsBroughtIntoViewWhenSelectedChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
      {
         TreeViewItem item = depObj as TreeViewItem;
         if (item == null)
            return;

         if (e.NewValue is bool == false)
            return;

         if ((bool)e.NewValue)
            item.Selected += OnTreeViewItemSelected;
         else
            item.Selected -= OnTreeViewItemSelected;
      }

      static void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
      {
         // Only react to the Selected event raised by the TreeViewItem
         // whose IsSelected property was modified. Ignore all ancestors
         // who are merely reporting that a descendant's Selected fired.
         if (!Object.ReferenceEquals(sender, e.OriginalSource))
            return;

         TreeViewItem item = e.OriginalSource as TreeViewItem;
         if (item != null)
            item.BringIntoView();
      }

      #endregion // IsBroughtIntoViewWhenSelected

      #region IsMouseDirectlyOverItem

      // IsMouseDirectlyOverItem:  A DependencyProperty that will be true only on the 
      // TreeViewItem that the mouse is directly over.  I.e., this won't be set on that 
      // parent item.
      //
      // This is the only public member, and is read-only.
      private static readonly DependencyPropertyKey IsMouseDirectlyOverItemKey =
                          DependencyProperty.RegisterAttachedReadOnly(
                                    "IsMouseDirectlyOverItem",
                                    typeof(bool),
                                    typeof(TreeViewItemBehavior),
                                    new FrameworkPropertyMetadata(null, CalculateIsMouseDirectlyOverItem));


      public static readonly DependencyProperty IsMouseDirectlyOverItemProperty = IsMouseDirectlyOverItemKey.DependencyProperty;

      public static bool GetIsMouseDirectlyOverItem(DependencyObject obj)
      {
         return (bool)obj.GetValue(IsMouseDirectlyOverItemProperty);
      }

      // A coercion method for the property
      private static object CalculateIsMouseDirectlyOverItem(DependencyObject item, object value)
      {
         // This method is called when the IsMouseDirectlyOver property is being calculated
         // for a TreeViewItem.  
         if (item == s_currentItem)
            return true;
         return false;
      }


      // UpdateOverItem:  A private RoutedEvent used to find the nearest encapsulating
      // TreeViewItem to the mouse's current position.
      private static readonly RoutedEvent UpdateOverItemEvent = EventManager.RegisterRoutedEvent(
            "UpdateOverItem", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TreeViewItemBehavior));

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
      static TreeViewItemBehavior()
      {
         // Get all Mouse enter/leave events for TreeViewItem.
         EventManager.RegisterClassHandler(typeof(TreeViewItem), TreeViewItem.MouseEnterEvent, new MouseEventHandler(OnMouseTransition), true);
         EventManager.RegisterClassHandler(typeof(TreeViewItem), TreeViewItem.MouseLeaveEvent, new MouseEventHandler(OnMouseTransition), true);

         // Listen for the UpdateOverItemEvent on all TreeViewItem's.
         EventManager.RegisterClassHandler(typeof(TreeViewItem), UpdateOverItemEvent, new RoutedEventHandler(OnUpdateOverItem));
      }

      // OnUpdateOverItem:  This method is a listener for the UpdateOverItemEvent.  When it is received,
      // it means that the sender is the closest TreeViewItem to the mouse (closest in the sense of the
      // tree, not geographically).
      private static void OnUpdateOverItem(object sender, RoutedEventArgs args)
      {
         // Mark this object as the tree view item over which the mouse
         // is currently positioned.
         s_currentItem = sender as TreeViewItem;
         // Tell that item to re-calculate the IsMouseDirectlyOverItem property
         s_currentItem.InvalidateProperty(IsMouseDirectlyOverItemProperty);

         // Prevent this event from notifying other tree view items higher in the tree.
         args.Handled = true;
      }


      // OnMouseTransition:  This method is a listener for both the MouseEnter event and
      // the MouseLeave event on TreeViewItems.  It updates the s_currentItem, and updates
      // the IsMouseDirectlyOverItem property on the previous TreeViewItem and the new
      // TreeViewItem.
      private static void OnMouseTransition(object sender, MouseEventArgs args)
      {
         lock (IsMouseDirectlyOverItemProperty)
         {
            if (s_currentItem != null)
            {
               // Tell the item that previously had the mouse that it no longer does.
               DependencyObject oldItem = s_currentItem;
               s_currentItem = null;
               oldItem.InvalidateProperty(IsMouseDirectlyOverItemProperty);
            }

            // Get the element that is currently under the mouse.
            IInputElement currentPosition = Mouse.DirectlyOver;

            // See if the mouse is still over something (any element, not just a tree view item).
            if (currentPosition != null)
            {
               // Yes, the mouse is over something.
               // Raise an event from that point.  If a TreeViewItem is anywhere above this point
               // in the tree, it will receive this event and update s_currentItem.
               RoutedEventArgs newItemArgs = new RoutedEventArgs(UpdateOverItemEvent);
               currentPosition.RaiseEvent(newItemArgs);
            }

         }
      }

      #endregion

      #region SelectOnRightClick

      #region Attached Property: SelectOnRightClick

      public static readonly DependencyProperty SelectOnRightClickProperty = DependencyProperty.RegisterAttached("SelectOnRightClick", typeof(bool), typeof(TreeViewItemBehavior), new FrameworkPropertyMetadata(false, OnSelectOnRightClickChanged));

      public static bool GetSelectOnRightClick(DependencyObject target)
      {
         return (bool)target.GetValue(SelectOnRightClickProperty);
      }

      public static void SetSelectOnRightClick(DependencyObject target, bool value)
      {
         target.SetValue(SelectOnRightClickProperty, value);
      }

      private static void OnSelectOnRightClickChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
      {
         OnSelectOnRightClickChanged(o, (bool)e.OldValue, (bool)e.NewValue);
      }

      private static void OnSelectOnRightClickChanged(DependencyObject o, bool oldValue, bool newValue)
      {
         TreeViewItem tvi = o as TreeViewItem;
         if (tvi != null)
         {
            if (!oldValue && newValue)
            {
               tvi.PreviewMouseRightButtonDown += tvi_PreviewMouseRightButtonDown;
            }
            else if (oldValue && !newValue)
            {
               tvi.PreviewMouseRightButtonDown -= tvi_PreviewMouseRightButtonDown;
            }
         }
      }

      static void tvi_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
      {
         Visual visual = e.OriginalSource as Visual;
         if (visual != null)
         {
            TreeViewItem tvi = visual.FindVisualParent<TreeViewItem>();
            if (tvi != null)
            {
               tvi.IsSelected = true;
               tvi.Focus();
               e.Handled = true;
            }
         }
      }

      #endregion



      #endregion
   }
}
