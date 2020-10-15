using System;
using System.Collections.Generic;
using System.Text;
using Kuvio.Kernel.AspNet.Blazor.Toast.Configuration;
using Microsoft.AspNetCore.Components;

namespace Kuvio.Kernel.AspNet.Blazor.Toast
{

    internal class Toast
    {
        public Guid Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public ToastSettings ToastSettings { get; set; }
        public ToastOptions Options { get; internal set; }
    }
}