using Newtonsoft.Json;

namespace Ibis.Features.Users;

public class User : Root<string>
{
    public User()
    {
        Id = Guid.NewGuid().ToString();
        UserId = Id;
        PrimaryLanguageId = string.Empty;
        DateCreated = DateTime.UtcNow;
        DateModified = DateTime.UtcNow;
        LanguagesSpoken = new();
        ActiveRooms = new();
    }

    public User(string id, string email, string? firstName = null, string? lastName = null) : this()
    {
        Id = id;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
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
    public Voice? Voice { get; private set; }
    public decimal Balance { get; private set; }
    public string PrimaryLanguageId { get; private set; }
    public List<Language> LanguagesSpoken { get; private set; }
    public List<ActiveRoom> ActiveRooms { get; private set; }


    internal void JoinRoom(string roomId)
    {
        if (!ActiveRooms.Any(x => x.RoomId == roomId))
            ActiveRooms.Add(new(roomId, DateTime.UtcNow));
    }

    internal string? LeaveRoom(string roomId)
    {
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

    internal Language? PrimaryLanguage => LanguagesSpoken.FirstOrDefault(x => x.Id == PrimaryLanguageId);

    internal void AddCharge(UserCharge userCharge)
    {
        Balance += userCharge.Amount;
    }

    internal void UpdateProfile(string fullName, string languageId, string? pronouns, string? description)
    {
        FirstName = fullName.Split(' ')[0];
        LastName = fullName.Split(' ')[1];
        PrimaryLanguageId = languageId;
        Pronouns = pronouns;
        Description = description;
    }
}

public record ActiveRoom(string RoomId, DateTime JoinDate);
