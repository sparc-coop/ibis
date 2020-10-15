using Kuvio.Kernel.Core;
using Kuvio.Kernel.Core.Common;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Kuvio.Kernel.Database.CosmosDb
{
    public class CosmosDbRepository<T> : IPartitionedRepository<T> where T : CosmosDbRootEntity
    {
        public IQueryable<T> Query { get; set; }
        Container Container { get; }
        string Discriminator { get; set; }
        public bool AllowSynchronousQueries { get; }
        private PartitionKey PartitionKey { get; set; }

        public CosmosDbRepository(CosmosDbContext context)
        {
            Container = context.Container<T>();
            Discriminator = typeof(T).Name;
            AllowSynchronousQueries = context.AllowSynchronousQueries;

            Query = Container.GetItemLinqQueryable<T>(AllowSynchronousQueries)
                .Where(x => x.Discriminator == Discriminator);
        }

        public async Task<T> FindAsync(string id)
        {
            return await Container.ReadItemAsync<T>(id, new PartitionKey(id));
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> expression)
        {
            var results = await Query.Where(expression).Take(1).ToListAsync();
            return results.FirstOrDefault();
        }


        public async Task<T> FindAsync(object id)
        {
            return await Container.ReadItemAsync<T>(id.ToString(), new PartitionKey(id.ToString()));
        }

      

        public async Task<List<T>> GetAllAsync()
        {
            var results = await Query.ToListAsync();
            return results.ToList();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> expression)
        {
            return await Query.Where(expression).CountAsync();
        }

        // Commands
        public async Task<T> AddAsync(T item)
        {  
            if (PartitionKey == PartitionKey.Null)
                return await Container.CreateItemAsync(item, new PartitionKey(item.PartitionKey));
            else
                return await Container.CreateItemAsync(item, PartitionKey);
        }

        public async Task UpdateAsync(T item)
        {
            if (PartitionKey == PartitionKey.Null)
                await Container.UpsertItemAsync(item, new PartitionKey(item.PartitionKey));
            else
                await Container.UpsertItemAsync(item, PartitionKey);
        }

        public async Task ExecuteAsync(string id, Action<T> action)
        {
            var entity = await FindAsync(id);
            await ExecuteAsync(entity, action);
        }

        public async Task ExecuteAsync(object id, Action<T> action)
        {
            var entity = await FindAsync(id.ToString());
            await ExecuteAsync(entity, action);
        }

        public Task CommitAsync()
        {
            throw new NotImplementedException();
        }

        public async Task ExecuteAsync(T entity, Action<T> action)
        {
            action(entity);
            await UpdateAsync(entity);
        }

        public async Task DeleteAsync(T item)
        {
            if (PartitionKey == null)
                await Container.DeleteItemAsync<T>(item.Id, new PartitionKey(item.PartitionKey));
            else
                await Container.DeleteItemAsync<T>(item.Id, PartitionKey);
        }

        public IPartitionedRepository<T> Partition(string partitionKey)
        {
            PartitionKey = new PartitionKey(partitionKey);
            var options = new QueryRequestOptions { PartitionKey = PartitionKey };
            Query = Container.GetItemLinqQueryable<T>(AllowSynchronousQueries, requestOptions: options)
                .Where(x => x.Discriminator == Discriminator);
            return this;
        }

    }
}
