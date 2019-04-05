using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Reflection;
using EnvDTE;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Threading;

namespace Alphaleonis.VSProjectSetMgr
{
   [Guid("4EC4A39E-1DBF-41F2-9A5A-410B9F4EDD4C")]
   [ComVisible(true)]
   public interface SInteractionService
   {
   }

   [Guid("6FEB8472-A386-46AA-B017-646DD7C37419")]
   [ComVisible(true)]
   public interface IInteractionService 
   {
      ProjectSetManagerUserOptions GetSettings();

      VSConstants.MessageBoxResult ShowDialog(string title, string message, OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON defaultbutton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON icon = OLEMSGICON.OLEMSGICON_INFO);
      void ShowError(string message, params object[] args);
      void ShowError(string message);
   }

   public class InteractionService : IInteractionService, SInteractionService
   {
      private readonly ISettingsProvider m_services;
      private readonly Lazy<string> m_packageTitle;

      public InteractionService(ISettingsProvider services)
      {         
         if (services == null)
            throw new ArgumentNullException("services", "services is null.");
         m_services = services;
         m_packageTitle = new Lazy<string>(() => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title);
      }

      public ProjectSetManagerUserOptions GetSettings()
      {
         return m_services.GetSettings();
      }

      public VSConstants.MessageBoxResult ShowDialog(string title, string message, OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON defaultbutton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON icon = OLEMSGICON.OLEMSGICON_INFO)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         IVsUIShell uiShell = (IVsUIShell)m_services.GetService<SVsUIShell>();
         Guid clsid = Guid.Empty;
         int result;
         Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                    0,
                    ref clsid,
                    title,
                    message,
                    String.Empty,
                    0,
                    buttons, 
                    defaultbutton,
                    icon,
                    0,        // false
                    out result));
         
         return (VSConstants.MessageBoxResult)result;
      }

      public void ShowError(string message, params object[] args)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         ShowError(String.Format(message, args));
      }

      public void ShowError(string message)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         IVsUIShell uiShell = (IVsUIShell)m_services.GetService<SVsUIShell>();
         Guid clsid = Guid.Empty;
         int result;
         Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                    0,
                    ref clsid,
                    String.Format("Error in {0}:", m_packageTitle.Value),
                    message,
                    String.Empty,
                    0,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    0,        // false
                    out result));

      }
   }
}
