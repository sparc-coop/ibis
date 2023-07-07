using Ibis._Plugins;
using System.Text;
using File = Sparc.Blossom.Data.File;

namespace Ibis.Rooms;

public record SourceMessage(string RoomId, string MessageId);
public class Room : Entity<string>
{
    public string RoomId { get; private set; }
    public string RoomType { get; private set; }
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public UserAvatar HostUser { get; private set; }
    public List<UserAvatar> Users { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? LastActiveDate { get; private set; }

    internal SourceMessage? SourceMessage { get; private set; }
    internal List<Language> Languages { get; private set; }
    internal DateTime? EndDate { get; private set; }
    internal AudioMessage? Audio { get; private set; }
    internal virtual List<Message> Messages { get; private set; } = new();

    public Room(string name, string type, User hostUser) : this()
    {
        SetName(name);
        RoomType = type;
        HostUser = hostUser.Avatar;
    }

    // For Ibis Support Rooms
    public bool? IsSupportRoom { get; private set; }
    public bool? ToBeResolved { get; private set; }
    public bool? IsResolved { get; private set; }
    public bool? IsGeneralSupport { get; private set; }
    public bool? IsBug { get; private set; }
    public bool? IsAccountIssue { get; private set; }

    private Room() 
    { 
        Id = Guid.NewGuid().ToString();
        RoomId = Id;
        RoomType = "Chat";
        Name = "";
        Slug = "";
        SetName("New Room");
        HostUser = new User().Avatar;
        Languages = new();
        StartDate = DateTime.UtcNow;
        LastActiveDate = DateTime.UtcNow;
        Users = new();
    }

    public void Join(User user)
    {
        var activeUser = Users.FirstOrDefault(x => x.Id == user.Id);
        if (activeUser == null)
        {
            activeUser = user.Avatar;
            Users.Add(activeUser);
        }

        if (user.PrimaryLanguage != null)
            AddLanguage(user.PrimaryLanguage);

        Broadcast(new UserJoined(Id, activeUser));
    }

    public void Leave(User user)
    {
        var activeUser = Users.FirstOrDefault(x => x.Id == user.Id);
        if (activeUser != null)
            Broadcast(new UserLeft(Id, activeUser));
    }

    public void SetName(string title)
    {
        Name = title;
        Slug = UrlFriendly(Name);
    }

    public void AddMessages(List<Message> messages)
    {
        Messages.AddRange(messages);
        LastActiveDate = DateTime.UtcNow;
        foreach (var message in messages)
            Broadcast(new MessageTextChanged(message));
    }

    public async Task AddFileAsync(IFileRepository<File> files, string filename, byte[] bytes)
    {
        var uploadFile = $"{RoomId}/upload/{filename}";
        File file = new("speak", uploadFile, AccessTypes.Public, new MemoryStream(bytes));
        await files.AddAsync(file);
        Broadcast(new FileUploaded(RoomId, file.Url!));
    }

    public async Task<AudioMessage> SpeakAsync(ISpeaker speaker, string language)
    {
        var messages = Messages.Where(x => x.Language == language)
            .OrderBy(x => x.Timestamp)
            .ToList();

        Audio = await speaker.SpeakAsync(messages);
        return Audio;
    }

    internal void Close()
    {
        EndDate = DateTime.UtcNow;
    }

    public record InviteUserRequest(string Email, string? Language);
    public async Task<UserAvatar> InviteUser(InviteUserRequest request, User invitingUser, IRepository<User> users, ITranslator translator, BlossomAuthenticator<User> authenticator, IConfiguration configuration, HttpRequest httpRequest, TwilioService twilio)
    {
        var user = users.Query.FirstOrDefault(u => u.Email == request.Email);
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
                dictionary[key] = await translator.TranslateAsync(dictionary[key], "en", language) ?? dictionary[key];
        }

        if (user == null)
        {
            user = new(request.Email);
            if (language != null)
            {
                await user.ChangeVoiceAsync(language, translator);
            }
            await users.AddAsync(user);
        }

        string roomLink = await authenticator.CreateMagicSignInLinkAsync(user.Email!, $"{configuration["WebClientUrl"]}/rooms/{Id}");
        roomLink = $"{httpRequest.Scheme}://{httpRequest.Host.Value}{roomLink}";

        var templateData = new
        {
            RoomName = Name,
            InvitingUser = invitingUser?.Avatar.Name ?? dictionary["YourFriend"],
            RoomLink = roomLink,
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

        await twilio.SendEmailTemplateAsync(request.Email, "d-24b6f07e97a54df2accc40a9789c0e23", templateData);

        return user.Avatar;
    }

    void AddLanguage(Language language)
    {
        if (Languages.Any(x => x.Id == language.Id))
            return;

        Languages.Add(language);
    }

    // Adopted from https://stackoverflow.com/a/25486
    static string UrlFriendly(string title)
    {
        if (title == null) return "";

        const int maxlen = 80;
        int len = title.Length;
        bool prevdash = false;
        var sb = new StringBuilder(len);
        char c;

        for (int i = 0; i < len; i++)
        {
            c = title[i];
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
            {
                sb.Append(c);
                prevdash = false;
            }
            else if (c >= 'A' && c <= 'Z')
            {
                // tricky way to convert to lowercase
                sb.Append((char)(c | 32));
                prevdash = false;
            }
            else if (c == ' ' || c == ',' || c == '.' || c == '/' ||
                c == '\\' || c == '-' || c == '_' || c == '=')
            {
                if (!prevdash && sb.Length > 0)
                {
                    sb.Append('-');
                    prevdash = true;
                }
            }
            else if ((int)c >= 128)
            {
                int prevlen = sb.Length;
                sb.Append(RemapInternationalCharToAscii(c));
                if (prevlen != sb.Length) prevdash = false;
            }
            if (i == maxlen) break;
        }

        if (prevdash)
            return sb.ToString()[..(sb.Length - 1)];
        else
            return sb.ToString();
    }

    private static string RemapInternationalCharToAscii(char c)
    {
        string s = c.ToString().ToLowerInvariant();
        if ("àåáâäãåą".Contains(s))
        {
            return "a";
        }
        else if ("èéêëę".Contains(s))
        {
            return "e";
        }
        else if ("ìíîïı".Contains(s))
        {
            return "i";
        }
        else if ("òóôõöøőð".Contains(s))
        {
            return "o";
        }
        else if ("ùúûüŭů".Contains(s))
        {
            return "u";
        }
        else if ("çćčĉ".Contains(s))
        {
            return "c";
        }
        else if ("żźž".Contains(s))
        {
            return "z";
        }
        else if ("śşšŝ".Contains(s))
        {
            return "s";
        }
        else if ("ñń".Contains(s))
        {
            return "n";
        }
        else if ("ýÿ".Contains(s))
        {
            return "y";
        }
        else if ("ğĝ".Contains(s))
        {
            return "g";
        }
        else if (c == 'ř')
        {
            return "r";
        }
        else if (c == 'ł')
        {
            return "l";
        }
        else if (c == 'đ')
        {
            return "d";
        }
        else if (c == 'ß')
        {
            return "ss";
        }
        else if (c == 'Þ')
        {
            return "th";
        }
        else if (c == 'ĥ')
        {
            return "h";
        }
        else if (c == 'ĵ')
        {
            return "j";
        }
        else
        {
            return "";
        }
    }
}
