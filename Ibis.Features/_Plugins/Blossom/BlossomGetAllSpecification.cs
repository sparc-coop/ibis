using Ardalis.Specification;

namespace Sparc.Blossom;

public class BlossomGetAllSpecification<T> : Specification<T> where T : Root<string>
{
    public BlossomGetAllSpecification(int? take = null)
    {
        if (take != null)
            Query.Take(take.Value);
    }
}