using Kuvio.Kernel.Core.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transcriber.Core.Results
{
    public class Result : CosmosDbRootEntity
    {
        public Result()
        {

        }
        public Result(string projectId):base(projectId)
        {
            Id = Guid.NewGuid().ToString();
            ProjectID = projectId;
            RegisterDate = DateTime.UtcNow;
        }
        [JsonProperty("id")]
        public override string Id { get; set; }
        public string ProjectID { get; set; }
        public string DisplayText { get; set; }
        public long Duration { get; set; }
        public long Offset { get; set; }
        public string RecognitionStatus { get; set; }
        public DateTime RegisterDate { get; set; }
        public List<NBest> NBest { get; set; }
    }
}
