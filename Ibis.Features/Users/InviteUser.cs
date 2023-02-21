namespace Ibis.Users;

public record InviteUserRequest(string Email, string RoomId, string? Language);
public class InviteUser : Feature<InviteUserRequest, UserAvatar?>
{
    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }
    public BlossomAuthenticator<User> Authenticator { get; }
    public IConfiguration Configuration { get; }
    public ITranslator Translator { get; }
    TwilioService Twilio { get; set; }

    public InviteUser(TwilioService twilio, 
        IRepository<Room> rooms, 
        IRepository<User> users,
        BlossomAuthenticator<User> authenticator, 
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
            var room = Rooms.Query.First(r => r.RoomId == request.RoomId);
            var invitingUser = await Users.GetAsync(User);
            var user = Users.Query.FirstOrDefault(u => u.Email == request.Email);
            var language = user?.PrimaryLanguage?.Id ?? request.Language;
                
            var dictionary = new Dictionary<string, string>
            {
                { "YourFriend", "Your friend" },
                { "GuessSentence", "Guess what..." },
                { "InvitationSentence", "has invited you to join them on Ibis!" },
                { "JoinSentence", "Click the button below to join them now" },
                { "JoinButton", "Join" },
                { "QuestionSentence", "What is Ibis?" },
                { "ExplanationSentence", "Ibis enables you to communicate in *your* language and communication style." },
                { "LearnButton", "Learn More" },
                { "HelpQuestion", "Need help joining? Questions about accounts or billing?" },
                { "HelpSentence", "We're here to help. Our customer service reps are available most of the time." },
                { "ContactUs", "Contact Us" },
                { "Unsubscribe", "Unsubscribe" },
                { "UnsubscribePreferences", "Unsubscribe Preferences" },
                { "Powered", "POWERED BY IBIS" },
            };

            if (language != null)
            {
                foreach (var key in dictionary.Keys)
                    dictionary[key] = await Translator.TranslateAsync(dictionary[key], "en", language) ?? dictionary[key];
            }

            if (user == null)
            {
                user = new(request.Email);
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

            string roomLink = await Authenticator.CreateMagicSignInLinkAsync(user, $"{Configuration["WebClientUrl"]}/rooms/{request.RoomId}");
            roomLink = $"{Request.Scheme}://{Request.Host.Value}{roomLink}";


            var templateData = new
            {
                RoomName = room.Name,
                InvitingUser = invitingUser?.Avatar.Name ?? dictionary["YourFriend"],
                RoomLink =  roomLink,
                GuessSentence = dictionary["GuessSentence"],
                InvitationSentence = dictionary["InvitationSentence"],
                JoinSentence = dictionary["JoinSentence"],
                JoinButton = dictionary["JoinButton"],
                QuestionSentence = dictionary["QuestionSentence"],
                ExplanationSentence = dictionary["ExplanationSentence"],
                LearnButton = dictionary["LearnButton"],
                HelpQuestion = dictionary["HelpQuestion"],
                HelpSentence = dictionary["HelpSentence"],
                ContactUs = dictionary["ContactUs"],
                Unsubscribe = dictionary["Unsubscribe"],
                UnsubscribePreferences = dictionary["UnsubscribePreferences"],
                Powered = dictionary["Powered"]
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
