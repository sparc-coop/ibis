using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kuvio.Kernel.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Transcriber.Core;

namespace IsisTranscriber.NETCore.Pages
{
    public class DashboardModel : PageModel
    {
        private IRepository<Project> _projectsRep;

        public List<Project> Projects { get; set; }

        public DashboardModel(IRepository<Project> projectsRep)
        {
            _projectsRep = projectsRep;
        }
        public async Task OnGetAsync()
        {
            var userId = User.Id();
            Projects = await _projectsRep.Query.Where(x => x.UserID == userId).ToListAsync();
        }

        
    }
}
