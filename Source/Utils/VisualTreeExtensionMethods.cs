using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Alphaleonis.VSProjectSetMgr
{
   public static class VisualTreeExtensionMethods
   {
      public static T FindLogicalParent<T>(this DependencyObject obj) where T : DependencyObject
      {
         DependencyObject parent = obj;
         while (parent != null)
         {
            T correctlyTyped = parent as T;
            if (correctlyTyped != null)
               return correctlyTyped;
            parent = LogicalTreeHelper.GetParent(parent);
         }

         return null;
      }

      public static TParent FindVisualParent<TParent>(this Visual element, Predicate<Visual> predicate = null) where TParent : Visual
      {
         Visual parent = element as Visual;
         while (parent != null)
         {
            TParent correctlyTyped = parent as TParent;
            if (correctlyTyped != null && (predicate == null || predicate(parent)))
            {
               return correctlyTyped;
            }

            Visual newParent = null;
            if (parent is Visual)
               newParent = System.Windows.Media.VisualTreeHelper.GetParent(parent) as Visual;

            if (newParent == null)
               newParent = LogicalTreeHelper.GetParent(parent) as Visual;

            parent = newParent;
         }
         return null;
      }

      public static T FindVisualDescendant<T>(this Visual element) where T : Visual
      {
         if (element == null)
            return null;

         if (element is T)
            return (T)element;

         Visual foundElement = null;

         if (element is FrameworkElement)
            (element as FrameworkElement).ApplyTemplate();

         for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
         {
            Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
            foundElement = FindVisualDescendant<T>(visual);

            if (foundElement != null)
               break;
         }

         return (T)foundElement;
      }
   }
}

