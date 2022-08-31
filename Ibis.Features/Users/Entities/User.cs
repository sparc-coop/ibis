using Newtonsoft.Json;

namespace Ibis.Features.Users;

public class User : Root<string>
{
    public User()
    {
        Id = string.Empty;
        UserId = Id;
        Color = "#ffffff";
        PrimaryLanguageId = string.Empty;
        DateCreated = DateTime.UtcNow;
        DateModified = DateTime.UtcNow;
        LanguagesSpoken = new();
        ActiveRooms = new();
    }

    public string UserId { get { return Id; } set { Id = value; } }
    private string? _email;
    public string? Email
    {
        get { return _email; }
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _email = null;
                return;
            }

            _email = value.Trim().ToLower();
        }
    }

    [JsonIgnore]
    public string FullName => $"{FirstName} {LastName}";
    [JsonIgnore]
    public string Initials => $"{FirstName?[0]}{LastName?[0]}";
    public string? PhoneNumber { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? DisplayName { get; private set; }
    public DateTime DateCreated { get; private set; }
    public DateTime DateModified { get; private set; }
    public string? CustomerId { get; private set; }
    public string? ProfileImg { get; private set; }
    public string? Pronouns { get; private set; }
    public string? Description { get; private set; }
    public string Color { get; private set; }
    public Voice? Voice { get; private set; }
    public decimal Balance { get; private set; }
    public string PrimaryLanguageId { get; private set; }
    public List<Language> LanguagesSpoken { get; private set; }
    public List<ActiveRoom> ActiveRooms { get; private set; }


    internal void JoinRoom(string roomId, string connectionId)
    {
        if (!ActiveRooms.Any(x => x.RoomId == roomId))
            ActiveRooms.Add(new(roomId, connectionId, DateTime.UtcNow));
    }

    internal string? LeaveRoom(string roomOrConnectionId)
    {
        var roomId = ActiveRooms.FirstOrDefault(x => x.RoomId == roomOrConnectionId || x.ConnectionId == roomOrConnectionId)?.RoomId;
        if (roomId == null) return null;

        ActiveRooms.RemoveAll(x => x.RoomId == roomId);
        return roomId;
    }

    internal void ChangeLanguage(Language language)
    {
        if (!LanguagesSpoken.Any(x => x.Id == language.Id))
            LanguagesSpoken.Add(language);

        PrimaryLanguageId = language.Id;
    }

    internal void AddCharge(UserCharge userCharge)
    {
        Balance += userCharge.Amount;
    }
}

public record ActiveRoom(string RoomId, string ConnectionId, DateTime JoinDate);
