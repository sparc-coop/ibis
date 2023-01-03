using System.Security.Claims;

namespace Ibis.Features.Users;

public static class UserRepositoryExtensions
{
    public static async Task<User?> GetAsync(this IRepository<User> repository, ClaimsPrincipal user)
    {
        var azureId = user.Id();
        if (azureId == null)
            return null;
        
        return repository.Query.FirstOrDefault(x => x.AzureB2CId == azureId);
    }
}
