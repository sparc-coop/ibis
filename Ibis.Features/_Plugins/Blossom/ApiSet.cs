using Ardalis.Specification;

namespace Sparc.Blossom;

public class ApiSet<T>(IRepository<T> repository) where T : Entity
{
    IEnumerable<ISpecification<T>> Queries { get; }

}

