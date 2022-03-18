using Sparc.Authentication.AzureADB2C;
using System.Security.Claims;

namespace Ibis.Features._Plugins
{
    public static class ClaimsPrincipalExtensions
    {
        public static string Language(this ClaimsPrincipal principal) => principal.Get("language") ?? "en-us";
    }
}
