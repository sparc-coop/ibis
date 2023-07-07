using Ibis._Plugins.Blossom;
using Ibis._Plugins.Speech;
using Ibis._Plugins.Translation;
using Microsoft.AspNetCore.Mvc;
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

    protected Room() 
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
        Slug = new UrlFriendly(Name).Value;
    }

    public void AddMessages(List<Message> messages)
    {
        Messages.AddRange(messages);
        LastActiveDate = DateTime.UtcNow;
        foreach (var message in messages)
            Broadcast(new MessageTextChanged(message));
    }

    public async Task AddFileAsync([FromServices]IFileRepository<File> files, IFormFile incomingFile)
    {
        var uploadFile = $"{RoomId}/upload/{incomingFile.FileName}";
        File file = new("speak", uploadFile, AccessTypes.Public, incomingFile.OpenReadStream());
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
}
