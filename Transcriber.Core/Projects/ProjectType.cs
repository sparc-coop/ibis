using System;
using System.Collections.Generic;
using System.Text;

namespace Transcriber.Core.Projects
{
    public class ProjectType
    {
        public string Value { get; private set; }
        private ProjectType(string value) { Value = value;}
        public static ProjectType Audio { get { return new ProjectType("Audio"); } }
        public static ProjectType Video { get { return new ProjectType("Video"); } }
    }
}
