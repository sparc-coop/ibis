namespace Ibis.Features.Rooms;

public class Room : SparcRoot<string>
{
    public string RoomId { get; set; }
    public string Name { get; set; }
    public string HostUserId { get; set; }
    public string? HostMessageId { get; set; }
    public string? HostRoomId { get; set; }
    public List<Language> Languages { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? LastActiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<ActiveUser> ActiveUsers { get; internal set; }
    public List<string> PendingUsers { get; set; }
    public string? AudioId { get; set; }

    private Room() 
    { 
        Id = Guid.NewGuid().ToString();
        RoomId = Id;
        Name = "New Room";
        HostUserId = "";
        Languages = new();
        StartDate = DateTime.UtcNow;
        LastActiveDate = DateTime.UtcNow;
        ActiveUsers = new();
        PendingUsers = new();
    }

    public Room(string name, string hostUserId) : this()
    {
        Name = name;
        HostUserId = hostUserId;
    }

    public Room(Room room, Message message) : this()
    {
        // Create a subroom from a message

        Name = room.Name;
        HostMessageId = message.Id;
        HostRoomId = room.Id;
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

    public void AddUser(string userId, string language, string? profileImg, string? phoneNumber = null)
    {
        if (!ActiveUsers.Any(x => x.UserId == userId))
            ActiveUsers.Add(new(userId, DateTime.UtcNow, language, profileImg, phoneNumber));
    }

    public void RemoveUser(string userId)
    {
        ActiveUsers.RemoveAll(x => x.UserId == userId);
    }

    public void SetAudio(string audioId) => AudioId = audioId;

    internal async Task TranslateAllAsync(List<Message> messages, ITranslator translator)
    { 
        
    }

    internal async Task<List<Message>> TranslateAsync(Message message, ITranslator translator)
    {
        var translatedMessages = await translator.TranslateAsync(message, Languages);

        // Add reference to all the new translated messages
        foreach (var translatedMessage in translatedMessages)
            message.AddTranslation(translatedMessage.Language, translatedMessage.Id);

        return translatedMessages;
    }
}

public record ActiveUser(string UserId, DateTime JoinDate, string Language, string? ProfileImg, string? PhoneNumber);