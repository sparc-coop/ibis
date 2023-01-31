using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Reflection.PortableExecutable;

namespace Ibis.Users;

[BindProperties]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
[AllowAnonymous]
public class IbisLoginModel : LoginModel
{
    public IbisLoginModel(PasswordlessAuthenticator<User> authenticator, TwilioService twilio) : base(authenticator)
    {
        Authenticator = authenticator;
        Twilio = twilio;
    }

    private PasswordlessAuthenticator<User> Authenticator { get; }
    private TwilioService Twilio { get; }
    public string? Message { get; set; }

    public override async Task<IActionResult> LoginAsync()
    {
        Error = null;
        Message = null;
        if (string.IsNullOrWhiteSpace(Email) || !new EmailAddressAttribute().IsValid(Email))
        {
            Error = "Please enter a valid email address.";
            return Page();
        }

        var link = await Authenticator.CreateMagicSignInLinkAsync(Email, ReturnUrl!);
        link = $"{Request.Scheme}://{Request.Host.Value}{link}";

        var templateData = new
        { 
            Link= link,
        };

            await Twilio.SendEmailTemplateAsync(Email, "d-f9f37ce3326b4b92bf5db74a0062cd6b", templateData);

        Message = "Check your email for a link to sign in!";
        return Page();
    }
}
