using Ibis.Features.Sparc.Realtime;

namespace Ibis.Features.Users;

public record UserAvatarUpdated(UserAvatar Avatar) : GroupNotification(Avatar.Id); 
public class User : SparcRoot<string>
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
        Avatar = new(Id, "");
    }

    public User(string email) : this()
    {
        Id = email;
        Email = email;
        Avatar = new(Id, email);
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

    public DateTime DateCreated { get; private set; }
    public DateTime DateModified { get; private set; }
    public string? CustomerId { get; private set; }
    public Voice? Voice { get; private set; }
    public decimal Balance { get; private set; }
    public UserAvatar Avatar { get; private set; }
    public string PrimaryLanguageId { get; private set; }
    public List<Language> LanguagesSpoken { get; private set; }
    public List<ActiveRoom> ActiveRooms { get; private set; }
    public string? PhoneNumber { get; private set; }


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

    internal void ChangeVoice(Language language, Voice voice)
    {
        if (!LanguagesSpoken.Any(x => x.Id == language.Id))
            LanguagesSpoken.Add(language);

        PrimaryLanguageId = language.Id;
        Voice = voice;
    }

    internal Language? PrimaryLanguage => LanguagesSpoken.FirstOrDefault(x => x.Id == PrimaryLanguageId);

    internal void AddCharge(UserCharge userCharge)
    {
        Balance += userCharge.Amount;
    }

    internal void UpdateAvatar(UserAvatar avatar)
    {
        Avatar.Id = Id;
        Avatar.Voice = Voice?.ShortName;
        Avatar.Language = avatar.Language;
        Avatar.ForegroundColor = avatar.ForegroundColor;
        Avatar.Pronouns = avatar.Pronouns;
        Avatar.Name = avatar.Name;
        Avatar.Description = avatar.Description;
        Avatar.SkinTone = avatar.SkinTone;
        Avatar.Emoji = avatar.Emoji;

        Broadcast(new UserAvatarUpdated(Avatar));
    }

    internal void GoOnline(string connectionId)
    {
        Avatar.IsOnline = true;
        Broadcast(new UserAvatarUpdated(Avatar));
    }

    internal void GoOffline()
    {
        Avatar.IsOnline = false;
        Broadcast(new UserAvatarUpdated(Avatar));
    }

    public static User System => new("system");
}

public record ActiveRoom(string RoomId, DateTime JoinDate);
