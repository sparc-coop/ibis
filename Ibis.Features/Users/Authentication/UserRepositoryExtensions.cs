using System.Security.Claims;

namespace Ibis.Users;

public static class UserRepositoryExtensions
{
    public static Task<User?> GetAsync(this IRepository<User> repository, ClaimsPrincipal user)
    {
        var id = user?.Id();
        if (id == null)
            return Task.FromResult<User?>(null);
        
        var result = repository.Query.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(result);
    }
}
