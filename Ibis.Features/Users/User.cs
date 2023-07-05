using Ibis._Plugins;
using System.Security.Claims;

namespace Ibis.Users;

public record UserAvatarUpdated(UserAvatar Avatar) : Notification(Avatar.Id);
public record BalanceChanged(string HostUserId, long Ticks) : Notification(HostUserId);
public record UserLanguageChanged(string UserId, string Language) : Notification(UserId);
public class User : BlossomUser
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
        BillingInfo = new();
    }

    public User(string email) : this()
    {
        Email = email;
        UserName = email.ToUpper();
        Avatar = new(Id, email);
    }

    public User(string azureId, string email) : this(email)
    {
        AzureB2CId = azureId;
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
    public string? SlackTeamId { get; private set; }
    public string? SlackUserId { get; private set; }
    public string? AzureB2CId { get; private set; }
    public UserBilling BillingInfo { get; private set; }
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

    public async Task<string?> ChangeVoice(string languageId, string? voiceName, ITranslator translator, ISpeaker speaker)
    {
        var language = await translator.GetLanguageAsync(languageId)
            ?? throw new Exception("Language not found!");

        var hasLanguageChanged = Avatar.Language != language.Id;
        
        if (!LanguagesSpoken.Any(x => x.Id == language.Id))
            LanguagesSpoken.Add(language);

        Avatar.Language = language.Id;
        Avatar.LanguageIsRTL = language.IsRightToLeft;

        if (voiceName != null)
        {
            var voices = await speaker.GetVoicesAsync(languageId);
            var voice = voices.FirstOrDefault(x => x.ShortName == voiceName)
                ?? throw new Exception("Voice doesn't match language!");

            Avatar.Voice = voice?.ShortName;
            Avatar.Dialect = voice?.Locale;
            Avatar.Gender = voice?.Gender;
        }

        Broadcast(new UserAvatarUpdated(Avatar));

        if (hasLanguageChanged)
            Broadcast(new UserLanguageChanged(Id, Avatar.Language));

        var speech = await GenerateVoiceSampleAsync(translator, speaker);
        return new(speech?.Url);
    }

    internal Language? PrimaryLanguage => LanguagesSpoken.FirstOrDefault(x => x.Id == Avatar.Language);

    public static User System => new("system");

    internal void Refill(long ticksToAdd)
    {
        BillingInfo.TicksBalance += ticksToAdd;
        Broadcast(new BalanceChanged(Id, BillingInfo.TicksBalance));
    }

    internal void AddCharge(CostIncurred costIncurred)
    {
        BillingInfo.TicksBalance -= costIncurred.Ticks;
        Broadcast(new BalanceChanged(Id, BillingInfo.TicksBalance));
    }

    internal void UpdateAvatar(UserAvatar avatar)
    {
        Avatar.Id = Id;
        Avatar.Voice = avatar.Voice;
        Avatar.Language = avatar.Language;
        Avatar.LanguageIsRTL = avatar.LanguageIsRTL;
        Avatar.BackgroundColor = avatar.BackgroundColor;
        Avatar.Pronouns = avatar.Pronouns;
        Avatar.Name = avatar.Name;
        Avatar.Description = avatar.Description;
        Avatar.SkinTone = avatar.SkinTone;
        Avatar.Emoji = avatar.Emoji;
        Avatar.HearOthers = avatar.HearOthers;
        Avatar.MuteMe = avatar.MuteMe;

        Broadcast(new UserAvatarUpdated(Avatar));
    }

    internal void SetUpBilling(string customerId, string currency)
    {
        BillingInfo.SetUpCustomer(customerId, currency);
    }

    internal void GoOnline(string connectionId)
    {
        Avatar.IsOnline = true;
        Broadcast(new UserAvatarUpdated(Avatar));
        if (BillingInfo != null)
            Broadcast(new BalanceChanged(Id, BillingInfo.TicksBalance));
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

    protected override void RegisterClaims()
    {
        AddClaim(ClaimTypes.Email, Email);
        AddClaim(ClaimTypes.GivenName, Avatar.Name);
        AddClaim("sub", AzureB2CId);
        AddClaim("Language", Avatar.Language);
    }

    private async Task<AudioMessage?> GenerateVoiceSampleAsync(ITranslator translator, ISpeaker speaker)
    {
        var testMessages = new List<string>
        {
            "Hi, nice to meet you!",
            "Hey, how are things going today?",
            "What time do you want to meet?",
            "Thanks, talk to you later!"
        };

        int index = new Random().Next(testMessages.Count);
        var testMessage = testMessages[index];

        var translation = await translator.TranslateAsync(testMessage, "en", PrimaryLanguage!.Id);
        var speech = await speaker.SpeakAsync(new Message("", this, translation!));
        return speech;
    }
}

public record ActiveRoom(string RoomId, DateTime JoinDate);
