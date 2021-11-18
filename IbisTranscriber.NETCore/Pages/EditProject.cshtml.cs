using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kuvio.Kernel.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Transcriber.Core.Results;

namespace IbisTranscriber.NETCore.Pages
{
    public class EditProjectModel : PageModel
    {
        private IRepository<Result> _resultRep;

        public EditProjectModel(IRepository<Result> resultRep)
        {
            _resultRep = resultRep;
        }
        public List<Result> Results { get; set; }
        public async Task OnGetAsync(string projectId)
        {
            Results = await _resultRep.Query.Where(x => x.ProjectID == projectId).ToListAsync();
            //ProjectId = projectId;
        }
    }
}
