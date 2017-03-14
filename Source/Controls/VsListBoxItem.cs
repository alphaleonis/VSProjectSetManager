using System;
using System.Collections.Generic;
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

namespace Alphaleonis.VSProjectSetMgr.Controls
{
   public class VsListBoxItem : ListBoxItem
   {
      static VsListBoxItem()
      {
         DefaultStyleKeyProperty.OverrideMetadata(typeof(VsListBoxItem), new FrameworkPropertyMetadata(typeof(VsListBoxItem)));
      }
   }
}

