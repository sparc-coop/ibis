using System.Security.Claims;

namespace Ibis.Features.Users;

public static class UserRepositoryExtensions
{
    public static Task<User?> GetAsync(this IRepository<User> repository, ClaimsPrincipal user)
    {
        var azureId = user.Id();
        if (azureId == null)
            return Task.FromResult<User?>(null);
        
        var result = repository.Query.FirstOrDefault(x => x.AzureB2CId == azureId);
        return Task.FromResult(result);
    }
}
