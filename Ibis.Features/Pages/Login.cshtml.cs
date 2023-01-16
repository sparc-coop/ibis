using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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

        await Twilio.SendEmailAsync(Email, "Your Ibis Magic Login Link", "Welcome back to Ibis! Click the link below to login to your account.\r\n\r\n" + link);

        Message = "Check your email for a link to sign in!";
        return Page();
    }
}
