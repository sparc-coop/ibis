using System;
using System.Collections.Generic;
using System.Text;
using Kuvio.Kernel.AspNet.Blazor.Toast.Configuration;
using Microsoft.AspNetCore.Components;

namespace Kuvio.Kernel.AspNet.Blazor.Toast
{

    public class ToastSettings
    {
        public ToastSettings(string heading, string message, string backgroundClass, string iconClass)
        {
            Heading = heading;
            Message = message;
            BackgroundClass = backgroundClass;
            IconClass = iconClass;
        }

        public string BackgroundClass { get; set; }
        public string Heading { get; set; }
        public string IconClass { get; set; }
        public string Message { get; set; }
    }
}