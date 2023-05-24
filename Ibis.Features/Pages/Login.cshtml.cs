using Ibis._Plugins;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Reflection.PortableExecutable;

namespace Ibis.Users;

[BindProperties]
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
[AllowAnonymous]
public class IbisLoginModel : LoginModel
{
    public IbisLoginModel(BlossomAuthenticator<User> authenticator, TwilioService twilio, IRepository<User> users, ITranslator translator) : base(authenticator)
    {
        Authenticator = authenticator;
        Twilio = twilio;
        Users = users;
        Translator = translator;
    }

    private BlossomAuthenticator<User> Authenticator { get; }
    private TwilioService Twilio { get; }
    private IRepository<User> Users { get; }
    private ITranslator Translator { get; }
    public string Message { get; set; } = "Welcome to Ibis! Please enter your email below.";
    public string Welcome { get; set; } = "Welcome to Ibis";
    public string EmailLabel { get; set; } = "Email Address";
    public string SignInButton { get; set; } = "Sign In";

    public override PageResult Page()
    {
        var language = RequestLanguage();
        if (language != null)
        {
            Task.Run(async () =>
            {
                Message = await Translator.TranslateAsync(Message, "en", language) ?? Message;
                Welcome = await Translator.TranslateAsync(Welcome, "en", language) ?? Welcome;
                EmailLabel = await Translator.TranslateAsync(EmailLabel, "en", language) ?? EmailLabel;
                SignInButton = await Translator.TranslateAsync(SignInButton, "en", language) ?? SignInButton;
                if (Error != null)
                    Error = await Translator.TranslateAsync(Error, "en", language) ?? Error;
            }).Wait();
        }

        return base.Page();
    }

    public override async Task<IActionResult> LoginAsync()
    {
        Error = null;
        Message = "Welcome to Ibis! Please enter your email below.";
        var language = RequestLanguage();

        if (string.IsNullOrWhiteSpace(Email) || !new EmailAddressAttribute().IsValid(Email))
        {
            Error = "Please enter a valid email address.";
            return Page();
        }

        var user = Users.Query.FirstOrDefault(x => x.Email == Email);
        if (user == null)
        {
            user = new(Email);
            if (language != null)
            {
                var userLanguage = await Translator.GetLanguageAsync(language);
                if (userLanguage != null)
                {
                    user.ChangeVoice(userLanguage);
                }
            }
            await Users.AddAsync(user);
        }

        language = user?.PrimaryLanguage?.Id ?? RequestLanguage() ?? "en";
        var link = await Authenticator.CreateMagicSignInLinkAsync(Email, ReturnUrl!);
        link = $"{Request.Scheme}://{Request.Host.Value}{link}";

        var dictionary = new Dictionary<string, string>
        {
            { "Hi", "Hi from Ibis!" },
            { "ClickTheLink", "Click the link below to log into your account." },
            { "LogIn", "Log In" },
            { "NeedSupport", "Need support? We're here to help. Our customer service reps are available most of the time." },
            { "ContactSupport", "Contact Support" }
        };

        if (language != null)
        {
            foreach (var key in dictionary.Keys)
                dictionary[key] = await Translator.TranslateAsync(dictionary[key], "en", language) ?? dictionary[key];
        }

        var templateData = new
        {
            Link = link,
            Hi = dictionary["Hi"],
            ClickTheLink = dictionary["ClickTheLink"],
            LogIn = dictionary["LogIn"],
            NeedSupport = dictionary["NeedSupport"],
            ContactSupport = dictionary["ContactSupport"]
        };

        await Twilio.SendEmailTemplateAsync(Email, "d-f9f37ce3326b4b92bf5db74a0062cd6b", templateData);

        Message = "Check your email for a link to sign in!";
        return Page();
    }

    private string? RequestLanguage()
    {
        return Request.Headers["Accept-Language"].ToString().Split(',').FirstOrDefault()?.Split('-').FirstOrDefault();
    }
}
