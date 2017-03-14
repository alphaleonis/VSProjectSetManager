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
using Alphaleonis.VSProjectSetMgr.ViewModels.Nodes;
using System.ComponentModel;
using System.IO;

namespace Alphaleonis.VSProjectSetMgr
{
   [ClassInterface(ClassInterfaceType.AutoDual)]
   [CLSCompliant(false), ComVisible(true)]
   [Guid("35406E47-1D09-48A9-88D1-1EF1FF60615B")]
   public class ProjectSetManagerUserOptions : DialogPage
   {
      public ProjectSetManagerUserOptions()
      {
         Storage = ProjectSetProfileStorage.Solution;
         EditWindowLeft = 100;
         EditWindowWidth = 300;
         EditWindowTop = 100;
         EditWindowHeight = 500;
      }

      [DisplayName("Storage Location")]
      [Description("Determines where to store the project set profiles created. If 'Solution' the settings are stored in the .suo file associated with your solution. If 'External File' a separate file with the same name as the solution, but with a vspsm extension is created to persist the profiles.")]
      [Category("General")]
      public ProjectSetProfileStorage Storage { get; set; }

      [Browsable(false)]
      public int EditWindowLeft { get; set; }
      [Browsable(false)]
      public int EditWindowWidth { get; set; }
      [Browsable(false)]
      public int EditWindowTop { get; set; }
      [Browsable(false)]
      public int EditWindowHeight { get; set; }
   }

   [ComVisible(true)]
   public enum ProjectSetProfileStorage
   {
      Solution,
      ExternalFile,
   }
}

