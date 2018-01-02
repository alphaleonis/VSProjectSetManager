using Alphaleonis.VSProjectSetMgr.ViewModels.Nodes;
using Newtonsoft.Json;
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

      public void SaveBinary(Stream stream)
      {
         using (BinaryWriter writer = new BinaryWriter(stream, Encoding.Unicode, true))
         {
            SaveBinary(writer);
         }
      }

      public void SaveBinary(BinaryWriter writer)
      {
         writer.Write(ProjectSets.Count);
         foreach (var projectSet in ProjectSets)
            projectSet.Serialize(writer);
      }

      public void SaveJson(Stream stream)
      {
         using (StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8, 4096, true))
         using (JsonWriter writer = new JsonTextWriter(streamWriter))
         {
            SaveJson(writer);
         }
      }

      public void SaveJson(JsonWriter writer)
      {
         JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings()
         {
            Formatting = Formatting.Indented            
         });
         serializer.Serialize(writer, m_projectSets);
      }

      public void LoadJson(string fileName, SolutionManager solMgr)
      {
         using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
         using (StreamReader streamReader = new StreamReader(fs, Encoding.UTF8, true, 4096, true))
         using (JsonReader reader = new JsonTextReader(streamReader))
         {
            JsonSerializer serializer = JsonSerializer.Create();
            var projectSets = serializer.Deserialize<ObservableCollection<ProjectSet>>(reader);
            if (projectSets == null)
            {
               projectSets = new ObservableCollection<ProjectSet>();
            }

            m_projectSets.Clear();
            foreach (var projectSet in projectSets)
               m_projectSets.Add(projectSet);
         }
      }

      public void LoadBinary(Stream stream, SolutionManager solMgr)
      {
         using (BinaryReader reader = new BinaryReader(stream, Encoding.Unicode, true))
         {
            LoadBinary(reader, solMgr);
         }
      }

      private void LoadBinary(BinaryReader reader, SolutionManager solMgr)
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
