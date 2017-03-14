using Alphaleonis.VSProjectSetMgr.ViewModels.Nodes;
using Alphaleonis.VSProjectSetMgr.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Alphaleonis.VSProjectSetMgr.Controls
 {
   public partial class EditProjectSetControl : UserControl
   {
      public EditProjectSetControl()
      {
         InitializeComponent();
      }

      private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
      {
         EditProjectSetViewModel vm = DataContext as EditProjectSetViewModel;
         if (vm != null)
         {
            vm.SelectedProject = e.NewValue as ProjectSetNodeViewModel;            
         }

      }

      private void UserControl_Loaded(object sender, RoutedEventArgs e)
      {
         txtName.SelectAll();
         FocusManager.SetFocusedElement(this, txtName);
      }
   }
}
