namespace Ibis.Features.Users;

public record UserAvatarUpdated(UserAvatar Avatar) : SparcNotification(Avatar.Id);
public record BalanceChanged(string HostUserId, decimal Amount) : SparcNotification(HostUserId);
public class User : SparcRoot<string>
{
    public User()
    {
        Id = Guid.NewGuid().ToString();
        UserId = Id;
        DateCreated = DateTime.UtcNow;
        DateModified = DateTime.UtcNow;
        LanguagesSpoken = new();
        ActiveRooms = new();
        Avatar = new(Id, "");
    }

    public User(string id, string email) : this()
    {
        Id = id;
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
    public string? SlackTeamId { get; private set; }
    public string? SlackUserId { get; private set; }
    public decimal Balance { get; private set; }
    public UserAvatar Avatar { get; private set; }
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

        Avatar.Language = language.Id;
        Avatar.Voice = voice.ShortName;
    }

    internal Language? PrimaryLanguage => LanguagesSpoken.FirstOrDefault(x => x.Id == Avatar.Language);

    public static User System => new("system", "system");

    internal void AddCharge(UserCharge userCharge)
    {
        Balance += userCharge.Amount;
        Broadcast(new BalanceChanged(Id, Balance));
    }

    internal void UpdateAvatar(UserAvatar avatar)
    {
        Avatar.Id = Id;
        Avatar.Voice = avatar.Voice;
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
        if (Balance != 0)
            Broadcast(new BalanceChanged(Id, Balance));
    }

    internal void GoOffline()
    {
        Avatar.IsOnline = false;
        Broadcast(new UserAvatarUpdated(Avatar));
    }

    internal void RegisterWithSlack(string team_id, string user_id)
    {
        SlackTeamId = team_id;
        SlackUserId = user_id;
    }
}

public record ActiveRoom(string RoomId, DateTime JoinDate);
