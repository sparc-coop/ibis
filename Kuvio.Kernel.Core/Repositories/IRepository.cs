using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Kuvio.Kernel.Core
{
    public static class RepositoryExtensions
    {
        public static Task<List<T>> ToListAsync<T>(this IQueryable<T> queryable)
        {
            return queryable.ToDynamicListAsync<T>();
        }
    }

    public interface IRepository<T>
    {
        IQueryable<T> Query { get; }
        Task<T> FindAsync(object id);

        // IQueryable Pass-Throughs
        Task<T> FindAsync(Expression<Func<T, bool>> expression);
        Task<int> CountAsync(Expression<Func<T, bool>> expression);
        Task<List<T>> GetAllAsync();

        // Commands
        Task<T> AddAsync(T item);
        Task UpdateAsync(T item);
        Task DeleteAsync(T item);
        Task ExecuteAsync(object id, Action<T> action);
        Task ExecuteAsync(T entity, Action<T> action);
        Task CommitAsync();
    }
}