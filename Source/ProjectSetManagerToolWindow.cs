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
using Alphaleonis.VSProjectSetMgr.Views;

namespace Alphaleonis.VSProjectSetMgr
{
   /// <summary>
   /// This class implements the tool window exposed by this package and hosts a user control.
   ///
   /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
   /// usually implemented by the package implementer.
   ///
   /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
   /// implementation of the IVsUIElementPane interface.
   /// </summary>
   [Guid("d752d527-283a-4c97-b41e-3ac631d9a010")]
   public class ProjectSetManagerToolWindow : ToolWindowPane
   {
      /// <summary>
      /// Standard constructor for the tool window.
      /// </summary>
      public ProjectSetManagerToolWindow() :
         base(null)
      {
         // Set the window title reading it from the resources.
         this.Caption = Resources.ToolWindowTitle;
         // Set the image that will appear on the tab of the window frame
         // when docked with an other window
         // The resource ID correspond to the one defined in the resx file
         // while the Index is the offset in the bitmap strip. Each image in
         // the strip being 16x16.
         this.BitmapResourceID = 301;
         this.BitmapIndex = 1;

         this.ToolBar = new CommandID(GuidList.guidLoadedProjectsProfileManagerCmdSet, PkgCmdIDList.ToolbarID);
         this.ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;         
      }

      protected override void Initialize()
      {
         base.Initialize();
         // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
         // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
         // the object returned by the Content property.
         ProjectSetManagerToolWindowControl control = new ProjectSetManagerToolWindowControl();
         control.DataContext = new ProjectSetManagerToolWindowViewModel(this);
         base.Content = control;
         return;
      }
   }


}
