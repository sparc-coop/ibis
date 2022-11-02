using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Ibis.Features.Users;

public class AzureAdB2CClaimsTransformation : IClaimsTransformation
{
    public AzureAdB2CClaimsTransformation(IRepository<User> users)
    {
        Users = users;
    }

    public IRepository<User> Users { get; }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.FindFirstValue("iss")?.Contains("b2c") == true)
        {
            var azureId = principal.Id();
            var user = Users.Query.Where(u => u.AzureB2CId == azureId).FirstOrDefault();
            if (user != null)
                return Task.FromResult(user.CreatePrincipal());
        }

        return Task.FromResult(principal);
    }
}

