using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IsisTranscriber.Extensions
{
    public static class AzureB2CExtensions
    {
        //public static void AddKuvioAuthentication(this IServiceCollection services, string b2cClientId, string b2cTenant, string b2cPolicy)
        //{
        //    services.AddAuthentication(o => o.DefaultAuthenticateScheme = AzureADB2CDefaults.CookieScheme)
        //        .AddAzureADB2C(options =>
        //        {
        //            options.Instance = "https://login.microsoftonline.com/tfp/";
        //            options.ClientId = b2cClientId;
        //            options.CallbackPath = "/signin-oidc";
        //            options.Domain = b2cTenant;
        //            options.SignUpSignInPolicyId = b2cPolicy;
        //        });
        //}

        public static void OnLogin(this AuthenticationBuilder builder, Action<ClaimsPrincipal> onLogin)
        {
            builder.Services.Configure<OpenIdConnectOptions>(AzureADB2CDefaults.OpenIdScheme, options => {
                options.Events.OnRemoteFailure = context => OnRemoteFailure(context);
                options.Events.OnTokenValidated = context => OnTokenValidatedAsync(context, onLogin);
            });

            builder.Services.AddClaimsPrincipalInjector();
        }

        public static Task OnRemoteFailure(RemoteFailureContext context)
        {
            context.HandleResponse();
            // Handle the error code that Azure AD B2C throws when trying to reset a password from the login page
            // because password reset is not supported by a "sign-up or sign-in policy"
            if (context.Failure is OpenIdConnectProtocolException && context.Failure.Message.Contains("AADB2C90118"))
            {
                context.Response.Redirect("AzureADB2C/Account/ResetPassword");
            }
            else if (context.Failure is OpenIdConnectProtocolException && context.Failure.Message.Contains("access_denied"))
            {
                context.Response.Redirect("/");
            }
            else
            {
                context.Response.Redirect("/Home/Error?message=" + context.Failure.Message);
            }
            return Task.FromResult(0);
        }

        public static void AddClaimsPrincipalInjector(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped(context => context.GetRequiredService<IHttpContextAccessor>()?.HttpContext?.User);
        }

        private static Task OnTokenValidatedAsync(Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext context, Action<ClaimsPrincipal> onLogin)
        {
            onLogin(context.Principal);
            return Task.CompletedTask;
        }

        //private static Task OnRedirectToIdentityProviderAsync(RedirectContext context, string b2cPolicy)
        //{
        //    var defaultPolicy = b2cPolicy;
        //    if (context.Properties.Items.TryGetValue("Policy", out var policy) && !policy.Equals(defaultPolicy))
        //    {
        //        context.ProtocolMessage.Scope = OpenIdConnectScope.OpenIdProfile;
        //        context.ProtocolMessage.ResponseType = OpenIdConnectResponseType.IdToken;
        //        context.ProtocolMessage.IssuerAddress = context.ProtocolMessage.IssuerAddress.ToLower().Replace(defaultPolicy.ToLower(), policy.ToLower());
        //        context.Properties.Items.Remove("Policy");
        //    }

        //    return Task.FromResult(0);
        //}
    }
}
