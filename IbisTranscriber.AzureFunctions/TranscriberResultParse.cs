using System;
using System.Collections.Generic;
using System.Text;

namespace IbisTranscriber.AzureFunctions
{

    public class TranscriberResultPage
    {
        public string DisplayText { get; set; }
        public int Duration { get; set; }
        public string Id { get; set; }
        public Nbest[] NBest { get; set; }
        public int Offset { get; set; }
        public string RecognitionStatus { get; set; }
    }

    public class Nbest
    {
        public float Confidence { get; set; }
        public string Display { get; set; }
        public string ITN { get; set; }
        public string Lexical { get; set; }
        public string MaskedITN { get; set; }
        public Words[] Words { get; set; }
    }

    public class Words
    {
        public int Duration { get; set; }
        public int Offset { get; set; }
        public string Word { get; set; }
    }

}
