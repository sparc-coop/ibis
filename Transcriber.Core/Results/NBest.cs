using Kuvio.Kernel.Core.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transcriber.Core.Results
{
    public class NBest
    {
        public NBest()
        {

        }
        public string Id { get; set; }
        public string ResultID { get; set; }
        public float Confidence { get; set; }
        public string Display { get; set; }
        public string ITN { get; set; }
        public string Lexical { get; set; }
        public string MaskedITN { get; set; }
        public List<Words> Words { get; set; }
    }
}
