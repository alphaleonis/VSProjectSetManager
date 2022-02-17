using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Alphaleonis.VSProjectSetMgr
{
   public interface IOutputWindow
   {
      IOutputWindowPane Debug { get; }

      void DeletePane(Guid id);
      IOutputWindowPane GetOrCreatePane(Guid id, string title, bool initiallyVisible = true);
      IOutputWindowPane General { get; }
   }

   public interface IOutputWindowPane
   {
      string Name { get; set; }
      TextWriter CreateTextWriter();
      void Clear();
      void Activate();
      void Hide();
      void WriteLine(string message);
      void WriteLine(string format, params object[] args);
   }

   internal class OutputWindowPane : IOutputWindowPane
   {
      private readonly IVsOutputWindowPane m_vsWindowPane;

      public OutputWindowPane(IVsOutputWindowPane vsWindowPane)
      {
         m_vsWindowPane = vsWindowPane;
      }

      public string Name
      {
         get
         {
            Dispatcher.CurrentDispatcher.VerifyAccess();
            string name = null;
            ErrorHandler.ThrowOnFailure(m_vsWindowPane.GetName(ref name));
            return name;
         }

         set
         {
            Dispatcher.CurrentDispatcher.VerifyAccess();
            ErrorHandler.ThrowOnFailure(m_vsWindowPane.SetName(value));
         }

      }

      public void Hide()
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         ErrorHandler.ThrowOnFailure(m_vsWindowPane.Hide());
      }

      public void Activate()
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         ErrorHandler.ThrowOnFailure(m_vsWindowPane.Activate());
      }

      public void Clear()
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         ErrorHandler.ThrowOnFailure(m_vsWindowPane.Clear());
      }

      public TextWriter CreateTextWriter()
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         return new OutputWindowTextWriter(m_vsWindowPane);
      }

      public void WriteLine(string message)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         ErrorHandler.ThrowOnFailure(m_vsWindowPane.OutputStringThreadSafe(message + Environment.NewLine));
      }

      public void WriteLine(string format, params object[] args)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         ErrorHandler.ThrowOnFailure(m_vsWindowPane.OutputStringThreadSafe(String.Format(format, args) + Environment.NewLine));
      }

      private class OutputWindowTextWriter : TextWriter
      {
         private readonly IVsOutputWindowPane m_outputPane;

         public OutputWindowTextWriter(IVsOutputWindowPane outputPane)
         {
            m_outputPane = outputPane;
         }

         public override Encoding Encoding
         {
            get { return Encoding.UTF8; }
         }

         public override void Write(string value)
         {
            Dispatcher.CurrentDispatcher.VerifyAccess();

            ErrorHandler.ThrowOnFailure(m_outputPane.OutputStringThreadSafe(value));
         }

         public override void WriteLine()
         {
            Dispatcher.CurrentDispatcher.VerifyAccess();

            ErrorHandler.ThrowOnFailure(m_outputPane.OutputStringThreadSafe(Environment.NewLine));
         }

         public override void WriteLine(string value)
         {
            Dispatcher.CurrentDispatcher.VerifyAccess();

            Write(value);
            WriteLine();
         }
      }
   }

   internal class OutputWindow : IOutputWindow
   {
      private readonly IServiceProvider m_serviceProvider;

      public IOutputWindowPane General
      {
         get
         {
            Dispatcher.CurrentDispatcher.VerifyAccess();

            return GetOrCreatePane(Microsoft.VisualStudio.VSConstants.GUID_OutWindowGeneralPane, "General");
         }
      }

      public IOutputWindowPane Debug
      {
         get
         {
            Dispatcher.CurrentDispatcher.VerifyAccess();
            return GetOrCreatePane(Microsoft.VisualStudio.VSConstants.GUID_OutWindowDebugPane, "Debug");
         }
      }

      public OutputWindow(IServiceProvider serviceProvider)
      {
         m_serviceProvider = serviceProvider;
      }

      public IOutputWindowPane GetOrCreatePane(Guid id, string title, bool initiallyVisible = true)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         IVsOutputWindowPane pane;

         var outputWindow = m_serviceProvider.GetService<SVsOutputWindow, IVsOutputWindow>();
         if (!ErrorHandler.Succeeded(outputWindow.GetPane(ref id, out pane)))
         {
            ErrorHandler.ThrowOnFailure(outputWindow.CreatePane(ref id, title, initiallyVisible ? 1 : 0, 1));
            ErrorHandler.ThrowOnFailure(outputWindow.GetPane(ref id, out pane));
         }

         return new OutputWindowPane(pane);
      }

      public void DeletePane(Guid id)
      {
         Dispatcher.CurrentDispatcher.VerifyAccess();

         var outputWindow = m_serviceProvider.GetService<SVsOutputWindow, IVsOutputWindow>();
         ErrorHandler.ThrowOnFailure(outputWindow.DeletePane(ref id));
      }
   }
}
