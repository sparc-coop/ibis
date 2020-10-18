using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IsisTranscriber.NETCore.Pages
{
    public class EditProjectModel : PageModel
    {
        [Parameter]
        public int ProjectId { get; set; }
        public void OnGet()
        {
        }
    }
}
