using Ibis.Features.Sparc.Realtime;

namespace Ibis.Features.Rooms;

public record SourceMessage(string RoomId, string MessageId);
public record UserJoined(string RoomId, UserAvatar User) : GroupNotification(RoomId);
public record UserLeft(string RoomId, UserAvatar User) : GroupNotification(RoomId);

public class Room : SparcRoot<string>
{
    public string RoomId { get; private set; }
    public string Name { get; private set; }
    public UserAvatar HostUser { get; private set; }
    public List<UserAvatar> Users { get; private set; }
    public SourceMessage? SourceMessage { get; private set; }
    public List<Language> Languages { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? LastActiveDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public AudioMessage? Audio { get; private set; }

    private Room() 
    { 
        Id = Guid.NewGuid().ToString();
        RoomId = Id;
        Name = "New Room";
        HostUser = new User().Avatar;
        Languages = new();
        StartDate = DateTime.UtcNow;
        LastActiveDate = DateTime.UtcNow;
        Users = new();
    }

    public Room(string name, User hostUser) : this()
    {
        Name = name;
        HostUser = hostUser.Avatar;
    }

    public Room(Room room, Message message) : this()
    {
        // Create a subroom from a message

        Name = room.Name;
        SourceMessage = new(room.Id, message.Id);
        //Languages = room.Languages;
        //ActiveUsers = room.ActiveUsers;
        //Translations = room.Translations;
    }

    public void AddLanguage(Language language)
    {
        if (Languages.Any(x => x.Id == language.Id))
            return;

        Languages.Add(language);
        Broadcast(new LanguageAdded(Id, language));
    }

    public void AddActiveUser(User user)
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

    public void RemoveActiveUser(User user)
    {
        var activeUser = Users.FirstOrDefault(x => x.Id == user.Id);
        if (activeUser != null)
            Broadcast(new UserLeft(Id, activeUser));
    }

    internal void InviteUser(User user)
    {
        if (!Users.Any(x => x.Id == user.Id))
            Users.Add(user.Avatar);
    }

    internal async Task<List<Message>> TranslateAsync(Message message, ITranslator translator, bool forceRetranslation = false)
    {
        var languagesToTranslate = forceRetranslation
            ? Languages
            : Languages.Where(x => !message.HasTranslation(x.Id)).ToList();

        if (!languagesToTranslate.Any())
            return new();

        var translatedMessages = await translator.TranslateAsync(message, languagesToTranslate);

        // Add reference to all the new translated messages
        foreach (var translatedMessage in translatedMessages)
            message.AddTranslation(translatedMessage.Language, translatedMessage.Id);

        return translatedMessages;
    }

    internal async Task SpeakAsync(ISpeaker speaker, List<Message> messages)
    {
        Audio = await speaker.SpeakAsync(messages);        
    }

    internal void Close()
    {
        EndDate = DateTime.UtcNow;
    }

    internal void Rename(string title)
    {
        Name = title;
    }

}
