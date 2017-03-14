using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Alphaleonis.VSProjectSetMgr.Converters
{
   public class LeftMarginMultiplierConverter : IValueConverter
   {
      public double Length { get; set; }

      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         var item = value as TreeViewItem;
         if (item == null)
            return new Thickness(0);

         return new Thickness(Length * GetDepth(item), 0, 0, 0);
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new System.NotImplementedException();
      }

      private static int GetDepth(TreeViewItem item)
      {
         TreeViewItem parent;
         while ((parent = GetParent(item)) != null)
         {
            return GetDepth(parent) + 1;
         }
         return 0;
      }

      private static TreeViewItem GetParent(TreeViewItem item)
      {
         var parent = VisualTreeHelper.GetParent(item);
         while (!(parent is TreeViewItem || parent is TreeView))
         {
            parent = VisualTreeHelper.GetParent(parent);
         }
         return parent as TreeViewItem;
      }
   }

   public class RootTreeviewConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (value == null)
            return null;

         return new object[] { value };
      }

      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}

