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
   class ProjectSetRepositoryViewModel : ObservableBase
   {
      private readonly IProjectSetRepository m_repository;
      private readonly ViewModelCollection<ProjectSetSummaryViewModel, ProjectSet> m_projects;

      public ProjectSetRepositoryViewModel(IProjectSetRepository repository)
      {
         m_repository = repository;
         m_projects = ViewModelCollection<ProjectSetSummaryViewModel, ProjectSet>.Create(m_repository.ProjectSets, ps => new ProjectSetSummaryViewModel(ps));
      }

      public ViewModelCollection<ProjectSetSummaryViewModel, ProjectSet> ProjectSets
      {
         get
         {
            return m_projects;
         }
      }
   }
}

