using Kuvio.Kernel.Core.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Transcriber.Core.Users;

namespace Transcriber.Core
{
    public class teste : CosmosDbRootEntity
    {
        public teste(string name) : base(name)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            PartitionKey = name;
        }

        [JsonProperty("id")]
        public override string Id { get; set; }
        public string Name { get; set; }
    }
    public class File : CosmosDbRootEntity
    {
        public File(User user, string type, string url, string size, string format, DateTime registerDate) :base(user.Id)
        {
            Id = Guid.NewGuid().ToString();
            UserID = user.Id;
            Type = type;
            Url = url;
            Size = size;
            Format = format;
            RegisterDate = registerDate;
        }

        [JsonProperty("id")]
        public override string Id { get; set; }
        public string UserID { get; set; }
        public User User { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string Size { get; set; }
        public string Format { get; set; }
        public DateTime RegisterDate { get; set; }
    }
}
