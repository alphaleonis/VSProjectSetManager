using Alphaleonis.VSProjectSetMgr.ViewModels.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Alphaleonis.VSProjectSetMgr
{
   [Guid("4E2C8D14-8EAF-4313-BAD0-7309CF7B1611")]
   [ComVisible(true)]
   public interface SProjectSetRepository
   {
   }

   [Guid("8583E3A3-B772-47CF-8A09-12AF1E974755")]
   [ComVisible(true)]
   interface IProjectSetRepository
   {
      bool IsSolutionOpen { get; }
      ObservableCollection<ProjectSet> ProjectSets { get; }
   }

   class ProjectSetRepository : ObservableBase, IProjectSetRepository, SProjectSetRepository
   {
      #region Private Fields

      private readonly ObservableCollection<ProjectSet> m_projectSets;
      private bool m_isSolutionOpen;

      #endregion

      public ProjectSetRepository()
      {
         m_projectSets = new ObservableCollection<ProjectSet>();
      }

      #region Properties

      public ObservableCollection<ProjectSet> ProjectSets
      {
         get 
         { 
            return m_projectSets; 
         }
      }

      public bool IsSolutionOpen
      {
         get
         {
            return m_isSolutionOpen;
         }

         set
         {
            if (SetValue(ref m_isSolutionOpen, value) && value == false)
            {
               m_projectSets.Clear();
            }
         }
   }
      #endregion

      #region Methods

      public void MarkAsMostRecentlyUsed(ProjectSet projectSet, int mruSize)
      {
         if (mruSize > 0)
         {
            int index = ProjectSets.IndexOf(projectSet);
            if (index >= mruSize)
            {
               ProjectSets.RemoveAt(index);
               ProjectSets.Insert(0, projectSet);
            }
         }
      }

      public void Save(Stream stream)
      {
         using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Unicode, true))
         {
            Save(writer);
         }
      }

      public void Save(BinaryWriter writer)
      {
         writer.Write(ProjectSets.Count);
         foreach (var projectSet in ProjectSets)
            projectSet.Serialize(writer);
      }

      public void Load(Stream stream, SolutionManager solMgr)
      {
         using (BinaryReader reader = new BinaryReader(stream, Encoding.Unicode, true))
         {
            Load(reader, solMgr);
         }
      }

      private void Load(BinaryReader reader, SolutionManager solMgr)
      {
         List<ProjectSet> projectSets = new List<ProjectSet>();
         int count = reader.ReadInt32();
         for (int i = 0; i < count; i++)
         {
            projectSets.Add(new ProjectSet(reader));               
         }

         if (ProjectSets.Count > 0)
            ProjectSets.Clear();

         foreach (var projectSet in projectSets)
            ProjectSets.Add(projectSet);
      }

      #endregion                    
   }   
}
