namespace Ibis.Features.Rooms;

public class Room : Root<string>
{
    public string RoomId { get; set; }
    public string Name { get; set; }
    public string HostUserId { get; set; }
    public string? HostMessageId { get; set; }
    public string? HostRoomId { get; set; }
    public List<Language> Languages { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? LastActiveDate { get; set; }
    public List<ActiveUser> ActiveUsers { get; internal set; }
    public List<Messages.Translation> Translations { get; private set; }
    public string? AudioId { get; set; }

    private Room() 
    { 
        Id = Guid.NewGuid().ToString();
        RoomId = Id;
        Name = "New Conversation";
        HostUserId = "";
        Languages = new();
        StartDate = DateTime.UtcNow;
        LastActiveDate = DateTime.UtcNow;
        ActiveUsers = new();
        Translations = new();
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
        Languages = room.Languages;
        ActiveUsers = room.ActiveUsers;
        Translations = room.Translations;
    }

    public void AddLanguage(string language)
    {
        if (Languages.Any(x => x.Name == language))
            return;

        Languages.Add(new(language));
    }

    public void AddUser(string userId, string language, string? phoneNumber = null)
    {
        if (!ActiveUsers.Any(x => x.UserId == userId))
            ActiveUsers.Add(new(userId, DateTime.UtcNow, language, phoneNumber));
    }

    public void RemoveUser(string userId)
    {
        ActiveUsers.RemoveAll(x => x.UserId == userId);
    }

    public void SetAudio(string audioId) => AudioId = audioId;
}

public record ActiveUser(string UserId, DateTime JoinDate, string Language, string? PhoneNumber);