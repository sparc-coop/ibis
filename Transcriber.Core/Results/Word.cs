using Kuvio.Kernel.Core.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transcriber.Core.Results
{
    public class Word
    {
        public string Id { get; set; }
        public string NBestID { get; set; }
        public long Duration { get; set; }
        public long Offset { get; set; }
        public string Text { get; set; }
    }
}