using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Kuvio.Kernel.Core
{
    //public static class DbRepositoryExtensions
    //{
    //    public static IQueryable<T> Include<T>(this IDbRepository<T> repository, params string[] path) where T : class
    //    {
    //        return repository.Include(path);
    //    }
    //}

    public interface IDbRepository<T> : IRepository<T>
    {
        IQueryable<T> Include(params string[] path);
        Task CommitAsync();
    }
}