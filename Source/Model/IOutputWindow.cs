using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string name = null;
            ErrorHandler.ThrowOnFailure(m_vsWindowPane.GetName(ref name));
            return name;
         }

         set
         {
            ErrorHandler.ThrowOnFailure(m_vsWindowPane.SetName(value));
         }

      }

      public void Hide()
      {
         ErrorHandler.ThrowOnFailure(m_vsWindowPane.Hide());
      }

      public void Activate()
      {
         ErrorHandler.ThrowOnFailure(m_vsWindowPane.Activate());
      }

      public void Clear()
      {
         ErrorHandler.ThrowOnFailure(m_vsWindowPane.Clear());
      }

      public TextWriter CreateTextWriter()
      {
         return new OutputWindowTextWriter(m_vsWindowPane);
      }

      public void WriteLine(string message)
      {
         ErrorHandler.ThrowOnFailure(m_vsWindowPane.OutputString(message + Environment.NewLine));
      }

      public void WriteLine(string format, params object[] args)
      {
         ErrorHandler.ThrowOnFailure(m_vsWindowPane.OutputString(String.Format(format, args) + Environment.NewLine));
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
            ErrorHandler.ThrowOnFailure(m_outputPane.OutputString(value));
         }

         public override void WriteLine()
         {
            ErrorHandler.ThrowOnFailure(m_outputPane.OutputString(Environment.NewLine));
         }

         public override void WriteLine(string value)
         {
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
            return GetOrCreatePane(Microsoft.VisualStudio.VSConstants.GUID_OutWindowGeneralPane, "General");
         }
      }

      public IOutputWindowPane Debug
      {
         get
         {
            return GetOrCreatePane(Microsoft.VisualStudio.VSConstants.GUID_OutWindowDebugPane, "Debug");
         }
      }

      public OutputWindow(IServiceProvider serviceProvider)
      {
         m_serviceProvider = serviceProvider;
      }

      public IOutputWindowPane GetOrCreatePane(Guid id, string title, bool initiallyVisible = true)
      {
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
         var outputWindow = m_serviceProvider.GetService<SVsOutputWindow, IVsOutputWindow>();
         ErrorHandler.ThrowOnFailure(outputWindow.DeletePane(ref id));
      }
   }
}
