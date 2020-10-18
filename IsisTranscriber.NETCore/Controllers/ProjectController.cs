using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IsisTranscriber.NETCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Transcriber.Core;
using Kuvio.Kernel.Core;
using IbisTranscriber.NETCore.ViewModels;

namespace IbisTranscriber.NETCore.Controllers
{
    public class ProjectController : Controller
    {
        private IWebHostEnvironment hostingEnvironment;
        private IRepository<Project> projectRep;

        public ProjectController(IWebHostEnvironment hostingEnvironment, IRepository<Project> projRep)
        {
            this.hostingEnvironment = hostingEnvironment;
            projectRep = projRep;
        }


        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Index(ProjectViewModel model)
        {
            Startup.Progress = 0;

            long totalBytes = model.Files.Sum(f => f.Length);

            //todo save new project
            
            Project newProject = new Project(User.Id(), model.Type, model.Name);
            await projectRep.AddAsync(newProject);

            foreach (IFormFile source in model.Files)
            {
                string filename = ContentDispositionHeaderValue.Parse(source.ContentDisposition).FileName.ToString().Trim('"');

                filename = this.EnsureCorrectFilename(filename);

                byte[] buffer = new byte[16 * 1024];

                using (FileStream output = System.IO.File.Create(this.GetPathAndFilename(filename)))
                {
                    using (Stream input = source.OpenReadStream())
                    {
                        long totalReadBytes = 0;
                        int readBytes;

                        while ((readBytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            await output.WriteAsync(buffer, 0, readBytes);
                            totalReadBytes += readBytes;
                            Startup.Progress = (int)((float)totalReadBytes / (float)totalBytes * 100.0);
                            await Task.Delay(10); // It is only to make the process slower
                        }
                    }
                }
            }

            return Ok("success");
            //return this.Content("success");
        }

        [HttpPost]
        public ActionResult Progress()
        {
            return this.Content(Startup.Progress.ToString());
        }

        private string EnsureCorrectFilename(string filename)
        {
            if (filename.Contains("\\"))
                filename = filename.Substring(filename.LastIndexOf("\\") + 1);

            return filename;
        }

        private string GetPathAndFilename(string filename)
        {
            string path = this.hostingEnvironment.WebRootPath + "\\uploads\\";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path + filename;
        }
    }
}
