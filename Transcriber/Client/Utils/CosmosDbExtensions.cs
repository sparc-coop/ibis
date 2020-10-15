﻿using Kuvio.Kernel.Database.CosmosDb;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transcriber.Client
{
    public static class CosmosDbExtensions
    {
        public static void AddCosmosContext<T>(this IServiceCollection services, CosmosDbOptions options)
            where T : CosmosDbContext
        {
           
            var connectionString = $"AccountEndpoint={options.Url};AccountKey={options.Key}";
            services.AddTransient(x => new CosmosClient(connectionString,new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway }).GetDatabase(options.Database));
            services.AddTransient<CosmosDbContext, T>();
        }
    }
}
