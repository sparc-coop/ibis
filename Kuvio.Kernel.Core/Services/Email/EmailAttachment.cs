using System;
using System.Collections.Generic;
using System.Text;

namespace Kuvio.Kernel.Core.Services.Email
{
    public class EmailAttachment
    {
        public string Filename { get; set; }
        public string Base64Content { get; set; }
    }
}
