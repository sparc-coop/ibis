using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IbisTranscriber.NETCore.ViewModels
{
    public class ProjectViewModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public IList<IFormFile> Files { get; set; }

    }
}
