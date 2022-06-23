using Newtonsoft.Json;

namespace Ibis.Features.Users;
public class User : Root<string>
{
    public User()
    {
        Id = string.Empty;
        UserId = Id;
        PrimaryLanguageId = string.Empty;
        DateCreated = DateTime.UtcNow;
        DateModified = DateTime.UtcNow;
        LanguagesSpoken = new();
        ActiveRooms = new();
        Color = SetColor();
    }

    public string UserId { get { return Id; } set { Id = value; } }
    private string? _email;
    public string? Email
    {
        get { return _email; }
        set
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                _email = null;
                return;
            }

            _email = value.Trim().ToLower();
        }
    }

    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }

    internal void JoinRoom(string roomId, string connectionId)
    {
        if (!ActiveRooms.Any(x => x.RoomId == roomId))
            ActiveRooms.Add(new(roomId, connectionId, DateTime.UtcNow));
    }

    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    [JsonIgnore]
    public string FullName => $"{FirstName} {LastName}";
    [JsonIgnore]
    public string Initials => $"{FirstName?[0]}{LastName?[0]}";
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
    public string CustomerId { get; set; }
    public string ProfileImg { get; set; }

    internal string? LeaveRoom(string roomOrConnectionId)
    {
        var roomId = ActiveRooms.FirstOrDefault(x => x.RoomId == roomOrConnectionId || x.ConnectionId == roomOrConnectionId)?.RoomId;
        if (roomId == null) return null;

        ActiveRooms.RemoveAll(x => x.RoomId == roomId);
        return roomId;
    }

    public string PrimaryLanguageId { get; set; }
    public List<Language> LanguagesSpoken { get; set; }
    public List<ActiveRoom> ActiveRooms { get; set; }
    public string? Color { get; set; }
    public string SetColor()
    {
        var hash = 0;
        int s = 50;
        int l = 65;

        if (Initials != null)
            for (var i = 0; i < Initials.Length; i++)
            {
                hash = Initials.ToCharArray().Sum(x => x) % 100 + ((hash << 5) - hash);
            }

        var h = hash % 360;
        return "hsl(" + h + ", " + s + "%, " + l + "%)";
    }
}

public record ActiveRoom(string RoomId, string ConnectionId, DateTime JoinDate);
