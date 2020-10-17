using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IsisTranscriber.NETCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Program.BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
          WebHost.CreateDefaultBuilder(args)
            //.UseContentRoot(Directory.GetCurrentDirectory())
        //    .UseKestrel(options =>
        //    {
        //        options.Limits.MaxRequestBodySize = 262144000; //250MB
        //})
            //.UseIISIntegration()
            .UseStartup<Startup>()
            .Build();
    }
}
