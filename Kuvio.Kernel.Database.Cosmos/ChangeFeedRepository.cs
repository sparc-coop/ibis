using Kuvio.Kernel.Core;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kuvio.Kernel.Database.CosmosDb
{
    public class ChangeFeedRepository<T> : IChangeFeedRepository<T>
    {
        Container Container { get; set; }
        Container LeaseContainer { get; set; }
        ChangeFeedProcessor ChangeFeedProcessor { get; set; }

        public ChangeFeedRepository(CosmosDbContext context)
        {
            Container = context.Container<T>();
            LeaseContainer = context.Container("leases");
        }

        public async Task<List<T>> GetChangesAsync(string datasetId, string checkpointId, int? limit)
        {
            return await GetChangesAsync(datasetId, checkpointId, limit, null);
        }

        public async Task<List<T>> GetChangesAsync(string datasetId, string checkpointId, int? limit, List<string> documentIds)
        {
            if (checkpointId == "0") checkpointId = null;

            var builder = Container.GetChangeFeedProcessorBuilder<T>("ChangeFeedProcessor", HandleChangesAsync)
                .WithInstanceName($"{datasetId}-{Guid.NewGuid()}")
                .WithLeaseContainer(LeaseContainer);

            if (limit != null)
                builder = builder.WithMaxItems(limit.Value);

            // Todo: Start time 
            var processor = builder.Build();
            await processor.StartAsync();

            return null;
        }

        static async Task HandleChangesAsync(IReadOnlyCollection<T> changes, CancellationToken cancellationToken)
        {
            foreach (var item in changes)
            {
                // I don't even know what to do here -- ideally return a stream to a websocket??
            }
        }
    }
}

