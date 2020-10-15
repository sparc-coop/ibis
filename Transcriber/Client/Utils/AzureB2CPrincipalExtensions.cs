using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Transcriber.Client.Utils
{
    public static class AzureB2CPrincipalExtensions
    {
        public static string DisplayName(this ClaimsPrincipal principal) => principal.Get("name") ?? principal.Get("http://schemas.microsoft.com/identity/claims/givenname") ?? principal.Get(ClaimTypes.Name);

        public static string Email(this ClaimsPrincipal principal) => principal.Get(ClaimTypes.Email) ?? principal.Get("emails");

        public static string FirstName(this ClaimsPrincipal principal) => principal.Get("given_name") ?? principal.Get("http://schemas.microsoft.com/identity/claims/givenname") ?? principal.Get(ClaimTypes.GivenName);
        public static string LastName(this ClaimsPrincipal principal) => principal.Get("family_name") ?? principal.Get("http://schemas.microsoft.com/identity/claims/lastname") ?? principal.Get("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname") ?? principal.Get("surname") ?? principal.Get(ClaimTypes.Name);
        public static string Initials(this ClaimsPrincipal principal)
        {
            if (!String.IsNullOrWhiteSpace(principal.FirstName()))
            {
                return principal.FirstName()?.FirstOrDefault().ToString() + principal.LastName()?.FirstOrDefault().ToString();
            }

            if (!String.IsNullOrWhiteSpace(DisplayName(principal)))
            {
                var initials = DisplayName(principal).Split(' ').Select(s => s[0]);

                return initials.First().ToString() + initials.Last().ToString();
            }

            return "";
        }

        public static string AzureID(this ClaimsPrincipal principal) => principal.Get("http://schemas.microsoft.com/identity/claims/objectidentifier");

        public static string Id(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(x => x.Type == "ID")?.Value;
        }

        public static string Get(this ClaimsPrincipal principal, string claimName) => principal.FindFirst(claimName)?.Value;
    }
}
