using Kuvio.Kernel.Core;
using Kuvio.Kernel.Core.Common;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;

namespace Kuvio.Kernel.Database.CosmosDb
{
    public class CosmosDbContext
    {
        Microsoft.Azure.Cosmos.Database Database { get; set; }
        protected static Dictionary<Type, string> ContainerMapping { get; set; }
        public bool AllowSynchronousQueries { get; set; }

        public CosmosDbContext(Microsoft.Azure.Cosmos.Database database) => Database = database;

        public static void Map<T>(string containerName) where T : CosmosDbRootEntity
        {
            if (ContainerMapping == null)
                ContainerMapping = new Dictionary<Type, string>();

            if (!ContainerMapping.ContainsKey(typeof(T)))
                ContainerMapping.Add(typeof(T), containerName);
        }

        public Container Container(string containerName)
        {
            return Database.GetContainer(containerName);
        }

        public Container Container<T>()
        {
            return Container(ContainerMapping[typeof(T)]);
        }
    }
}
