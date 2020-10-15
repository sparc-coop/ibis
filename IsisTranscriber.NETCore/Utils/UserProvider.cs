using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IsisTranscriber.NETCore
{
    public class UserProvider
    {
        public UserProvider(AuthenticationStateProvider provider)
        {
            Provider = provider;
        }

        public async Task<ClaimsPrincipal> GetPrincipal() => (await Provider.GetAuthenticationStateAsync()).User;
        public AuthenticationStateProvider Provider { get; }
    }
}
