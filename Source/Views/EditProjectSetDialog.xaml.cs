using Alphaleonis.VSProjectSetMgr.ViewModels.Nodes;
using EnvDTE;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Alphaleonis.VSProjectSetMgr.Views
{

   public partial class EditProjectSetDialog : DialogWindow
   {
      public EditProjectSetDialog()
      {
         InitializeComponent();
      }

      internal static bool? ShowDialog(IServiceProvider services, ProjectSetViewModel projectSet, string dialogTitle, Func<bool> beforeAccept)
      {
         EditProjectSetDialog dialog = new EditProjectSetDialog();
         dialog.Title = dialogTitle;

         Package pkg = services.GetService<Package>();

         try
         {
            IInteractionService isvc = (IInteractionService)services.GetService<SInteractionService>();
            ProjectSetManagerUserOptions settings = isvc.GetSettings();
            settings.LoadSettingsFromStorage();

            dialog.Width = settings.EditWindowWidth;
            dialog.Height = settings.EditWindowHeight;
            //dialog.Left = settings.EditWindowLeft;
            //dialog.Top = settings.EditWindowTop;
         }
         catch
         {
         }

         EditProjectSetViewModel viewModel = new EditProjectSetViewModel(services, projectSet);

         viewModel.CloseDialog += (s, e) =>
         {
            if (e.Result == true && (beforeAccept != null && beforeAccept() || beforeAccept == null) || e.Result != true)
            {
               try
               {
                  IInteractionService isvc = (IInteractionService)services.GetService<SInteractionService>();
                  ProjectSetManagerUserOptions settings = isvc.GetSettings();
                  settings.EditWindowWidth = (int)dialog.ActualWidth;
                  settings.EditWindowHeight = (int)dialog.ActualHeight;
                  settings.EditWindowLeft = (int)dialog.Left;
                  settings.EditWindowTop = (int)dialog.Top;
                  settings.SaveSettingsToStorage();
               }
               catch
               {
               }

               //winStates.SaveSettingsToStorage();
               dialog.DialogResult = e.Result;
               dialog.Close();
            }
         };

         dialog.DataContext = viewModel;
         return dialog.ShowModal();
      }

      private void DialogWindow_Loaded(object sender, RoutedEventArgs e)
      {
         // make sure it's in the current view space
         if (this.Top + (this.Height / 2)
             > (SystemParameters.VirtualScreenHeight + SystemParameters.VirtualScreenTop))
         {
            this.Top = SystemParameters.VirtualScreenHeight + SystemParameters.VirtualScreenTop - this.Height;
         }

         if (this.Left + (this.Width / 2)
             > (SystemParameters.VirtualScreenWidth + SystemParameters.VirtualScreenLeft))
         {
            this.Left = SystemParameters.VirtualScreenWidth + SystemParameters.VirtualScreenLeft - this.Width;
         }

         if (this.Top < SystemParameters.VirtualScreenTop)
         {
            this.Top = SystemParameters.VirtualScreenTop;
         }

         if (this.Left < SystemParameters.VirtualScreenLeft)
         {
            this.Left = SystemParameters.VirtualScreenLeft;
         }
      }
      
   }
}
