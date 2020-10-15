using Newtonsoft.Json;
using System;

namespace Kuvio.Kernel.Core.Common
{
    public class CosmosDbRootEntity : IIdentifiable
    {
        public CosmosDbRootEntity()
        {
            Id = Guid.NewGuid().ToString();
            PartitionKey = Id;
        }

        public CosmosDbRootEntity(string partitionKey) : this() => PartitionKey = partitionKey;

        [JsonProperty("id")]
        public virtual string Id { get; set; }
        public string PartitionKey { get; set; }
        public string Discriminator
        {
            get => GetType().Name;
            set
            {
                var type = GetType().Name;
                if (type != value)
                {
                    throw new TypeAccessException($"Discriminator {type} does not match persisted value of {value}.");
                }
            }
        }
    }
}
