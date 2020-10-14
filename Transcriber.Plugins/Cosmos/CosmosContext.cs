
using Kuvio.Kernel.Database.CosmosDb;
using System;
using System.Collections.Generic;
using System.Text;
using Transcriber.Core.Files;
using Transcriber.Core.Users;

namespace Transcriber.Plugins.Cosmos
{
    public class CosmosContext : CosmosDbContext
    {
        public CosmosContext(Microsoft.Azure.Cosmos.Database database) : base(database)
        {
            AllowSynchronousQueries = true;
                        
            Map<User>("User");
            Map<File>("File");
        }
    }
}
