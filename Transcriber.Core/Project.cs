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
        public Project(User user, 
            string type, 
            string name,
            string url, 
            string size, 
            string format, 
            string duration,
            string language,
            DateTime registerDate) : base(user.Id)
        {
            Id = Guid.NewGuid().ToString();
            UserID = user.Id;
            Type = type;
            Name = name;
            Url = url;
            Size = size;
            Format = format;
            Duration = duration;
            OriginLanguage = language;
            RegisterDate = registerDate;
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
    }
}
