using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kuvio.Kernel.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Transcriber.Core;

namespace IbisTranscriber.NETCore.Pages
{
    public class CreateProjectModel : PageModel
    {
        private readonly IRepository<Project> projectRep;
        public Project Project { get; set; }
        public CreateProjectModel(IRepository<Project> projectRep)
        {
            this.projectRep = projectRep;
        }

        public void OnGet()
        {
        }
    }
}
