using Ibis._Plugins;
using Markdig.Syntax;
using Microsoft.AspNetCore.Identity;
using Sparc.Ibis;
using System.Reflection.Metadata;

namespace Ibis.Users;

public record InviteUserRequest(string Email, string RoomId, string? Language);
public class InviteUser : Feature<InviteUserRequest, UserAvatar?>
{
    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }
    public PasswordlessAuthenticator<User> Authenticator { get; }
    public IConfiguration Configuration { get; }
    public ITranslator Translator { get; }
    TwilioService Twilio { get; set; }

    public InviteUser(TwilioService twilio, 
        IRepository<Room> rooms, 
        IRepository<User> users, 
        PasswordlessAuthenticator<User> authenticator, 
        IConfiguration configuration,
        ITranslator translator)
    {
        Twilio = twilio;
        Rooms = rooms;
        Users = users;
        Authenticator = authenticator;
        Configuration = configuration;
        Translator = translator;
    }

    public override async Task<UserAvatar?> ExecuteAsync(InviteUserRequest request)
    {
        try
        {
            var room = Rooms.Query.Where(r => r.RoomId == request.RoomId).First();
            var invitingUser = await Users.GetAsync(User);
            var user = Users.Query.Where(u => u.Email == request.Email).FirstOrDefault();
            var exampleTranslation = "Your friend";
            var guessSentence = await Translator.TranslateAsync("Guess what...", "en", request.Language);
            var invitationSentence = await Translator.TranslateAsync("has invited you to join them on Ibis!", "en", request.Language); ;
            var joinSentence = await Translator.TranslateAsync("Click the button below to join them now", "en", request.Language); ;
            var joinButton = await Translator.TranslateAsync("Join", "en", request.Language); ;
            var questionSentence = await Translator.TranslateAsync("What is Ibis?", "en", request.Language); ;
            var explanationSentence = await Translator.TranslateAsync("Ibis enables you to communicate in *your* language and communication style.", "en", request.Language); ;
            var learnButton = await Translator.TranslateAsync("Learn More", "en", request.Language); ;
            var helpQuestion = await Translator.TranslateAsync("Need help joining? Questions about accounts or billing?", "en", request.Language); ;
            var helpSentence = await Translator.TranslateAsync("We're here to help. Our customer service reps are available most of the time.", "en", request.Language); ;
            var contactUs = await Translator.TranslateAsync("Contact Us", "en", request.Language); ;
            var unsubscribe = await Translator.TranslateAsync("Unsubscribe", "en", request.Language); ;
            var unsubscribePreferences = await Translator.TranslateAsync("Unsubscribe Preferences", "en", request.Language); ;
            var powered = await Translator.TranslateAsync("POWERED BY IBIS", "en", request.Language); ;


            if (user == null)
            {
                user = new(request.Email);
                if (request.Language != null)
                {
                    var language = await Translator.GetLanguageAsync(request.Language);
                    if (language != null)
                    {
                        user.ChangeVoice(language);
                        exampleTranslation = await Translator.TranslateAsync(exampleTranslation, "en", request.Language);
                    }
                }
                await Users.AddAsync(user);
            }

            string roomLink = await Authenticator.CreateMagicSignInLinkAsync(user, $"{Configuration["WebClientUrl"]}/rooms/{request.RoomId}");
            roomLink = $"{Request.Scheme}://{Request.Host.Value}{roomLink}";


            var templateData = new
            {
                RoomName = room.Name,
                InvitingUser = invitingUser?.Avatar.Name ?? exampleTranslation,
                RoomLink =  roomLink,
                GuessSentence = guessSentence,
                InvitationSentence = invitationSentence,
                JoinSentence = joinSentence,
                JoinButton = joinButton,
                QuestionSentence = questionSentence,
                ExplanationSentence = explanationSentence,
                LearnButton = learnButton,
                HelpQuestion = helpQuestion,
                HelpSentence = helpSentence,
                ContactUs = contactUs,
                Unsubscribe = unsubscribe,
                UnsubscribePreferences = unsubscribePreferences,
                Powered = powered
            };

            await Twilio.SendEmailTemplateAsync(request.Email, "d-24b6f07e97a54df2accc40a9789c0e23", templateData);

            return user.Avatar;

        } catch (Exception ex)
        {
            var test = ex.Message;
            return null;
        }

    }
}
