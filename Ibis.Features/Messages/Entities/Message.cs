namespace Ibis.Features.Messages;

public class Message : SparcRoot<string>
{
    public string RoomId { get; private set; }
    public string UserId { get; private set; }
    public string Language { get; private set; }
    public DateTime Timestamp { get; private set; }
    public long? Duration { get; private set; }
    public string? Text { get; private set; }
    public string? AudioUrl { get; private set; }
    public Voice? Voice { get; private set; }
    public string UserName { get; set; }
    public string UserInitials { get; set; }
    public string? Color { get; set; }
    public string? VideoUrl { get; set; }
    public List<MessageTranslation>? Translations { get; set; }

    protected Message()
    {
        Id = Guid.NewGuid().ToString();
        RoomId = "";
        UserId = "";
        Language = "";
        UserName = "";
        UserInitials = "";
    }

    public Message(string roomId, User user, string text) : this()
    {
        RoomId = roomId;
        UserId = user.Id;
        UserName = user.FullName;
        UserInitials = user.Initials;
        Language = user.PrimaryLanguageId;
        Voice = user.Voice;
        Timestamp = DateTime.UtcNow;
        SetText(text);
    }

    public Message(Message sourceMessage, string toLanguage, string text) : this()
    {
        RoomId = sourceMessage.RoomId;
        UserId = sourceMessage.UserId;
        UserName = sourceMessage.UserName;
        UserInitials = sourceMessage.UserInitials;
        Language = toLanguage;
        SetText(text);
    }

    public void SetText(string text)
    {
        Text = text;
        Broadcast(new MessageTextChanged(RoomId, Id, Text));
    }
    public void SetAudio(string audioId) => AudioUrl = audioId;
    public void SetVideo(string videoId) => VideoUrl = videoId;
    
    internal void SetTimestamp(long offsetInTicks, TimeSpan duration)
    {
        Timestamp = Timestamp.AddTicks(offsetInTicks);
        Duration = duration.Ticks;
    }

    internal async Task SpeakAsync(ISynthesizer engine)
    {
        if (Voice == null)
            return;

        AudioUrl = await engine.SpeakAsync(this);
    }

    internal bool HasTranslation(string languageId)
    {
        return Translations != null && Translations.Any(x => x.Language == languageId);
    }
    
    internal void AddTranslation(string languageId, string messageId)
    {
        Translations ??= new();

        if (!HasTranslation(languageId))
            Translations.Add(new(languageId, messageId));
    }
}
