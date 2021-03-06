
using Kuvio.Kernel.Database.CosmosDb;
using System;
using System.Collections.Generic;
using System.Text;
using Transcriber.Core;
using Transcriber.Core.Results;
using Transcriber.Core.Users;

namespace Transcriber.Plugins.Cosmos
{
    public class CosmosContext : CosmosDbContext
    {
        public CosmosContext(Microsoft.Azure.Cosmos.Database database) : base(database)
        {
            AllowSynchronousQueries = true;

            Map<Project>("Project");
            Map<User>("User");
            Map<Result>("Result");
        }
    }
}
