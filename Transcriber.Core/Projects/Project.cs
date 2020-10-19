using Kuvio.Kernel.Core.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Transcriber.Core.Users;

namespace Transcriber.Core
{
    public class Project : CosmosDbRootEntity
    {
        public Project()
        {

        }
        public Project(string userId, 
            string type, 
            string name,
            string url = null, 
            string size = null, 
            string format = null, 
            string duration = null,
            string language = null) : base(userId)
        {
            Id = Guid.NewGuid().ToString();
            UserID = userId;
            Type = type;
            Name = name;
            Url = url;
            Size = size;
            Format = format;
            Duration = duration;
            OriginLanguage = language;
            RegisterDate = DateTime.UtcNow;
        }

        [JsonProperty("id")]
        public override string Id { get; set; }
        public string UserID { get; set; }
        public User User { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string Size { get; set; }
        public string Format { get; set; }
        public string Duration { get; set; }
        public string OriginLanguage { get; set; }
        public DateTime RegisterDate { get; set; }
        public string Status { get; set; }
    }
}
