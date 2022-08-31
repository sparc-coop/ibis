using Ibis.Features.Sparc.Realtime;

namespace Ibis.Features.Rooms;

public record SourceMessage(string RoomId, string MessageId);
public class Room : SparcRoot<string>
{
    public string RoomId { get; private set; }
    public string Name { get; private set; }
    public UserSummary HostUser { get; private set; }
    public List<UserSummary> ActiveUsers { get; private set; }
    public List<UserSummary> PendingUsers { get; private set; }
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
        HostUser = new("");
        Languages = new();
        StartDate = DateTime.UtcNow;
        LastActiveDate = DateTime.UtcNow;
        ActiveUsers = new();
        PendingUsers = new();
    }

    public Room(string name, User hostUser) : this()
    {
        Name = name;
        HostUser = new(hostUser);
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

    public void AddUser(User user)
    {
        if (!ActiveUsers.Any(x => x.Id == user.Id))
            ActiveUsers.Add(new(user));
    }

    public void RemoveUser(User user)
    {
        ActiveUsers.RemoveAll(x => x.Id == user.Id);
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
