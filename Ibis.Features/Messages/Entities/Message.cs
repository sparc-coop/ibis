using Ibis.Features.Sparc.Realtime;

namespace Ibis.Features.Messages;

public record MessageTranslation(string LanguageId, string MessageId);
public record Word(long Offset, long Duration, string Text);
public class Message : SparcRoot<string>
{
    public string RoomId { get; private set; }
    public string? SourceMessageId { get; private set; }
    public string Language { get; protected set; }
    public DateTime Timestamp { get; private set; }
    public UserAvatar User { get; private set; }
    public AudioMessage? Audio { get; private set; }
    public string? Text { get; private set; }
    public List<MessageTranslation>? Translations { get; private set; }
    public decimal Charge { get; private set; }

    protected Message()
    {
        Id = Guid.NewGuid().ToString();
        RoomId = "";
        User = new User().Avatar;
        Language = "";
    }

    public Message(string roomId, User user, string text) : this()
    {
        RoomId = roomId;
        User = user.Avatar;
        Language = user.PrimaryLanguageId;
        Audio = new(null, 0, user.Voice);
        Timestamp = DateTime.UtcNow;
        SetText(text);
    }

    public Message(Message sourceMessage, string toLanguage, string text) : this()
    {
        RoomId = sourceMessage.RoomId;
        SourceMessageId = sourceMessage.Id;
        User = new(sourceMessage.User);
        Audio = sourceMessage.Audio?.Voice == null ? null : new(null, 0, new(sourceMessage.Audio.Voice));
        Language = toLanguage;
        SetText(text);
    }

    public void SetText(string text)
    {
        if (Text == text)
            return;
        
        Text = text;
        Broadcast(new MessageTextChanged(this));
    }

    internal async Task SpeakAsync(ISpeaker engine)
    {
        if (Audio?.Voice == null)
            return;

        Audio = await engine.SpeakAsync(this);
        Broadcast(new MessageAudioChanged(this));
    }

    internal bool HasTranslation(string languageId)
    {
        return Language == languageId
            || (Translations != null && Translations.Any(x => x.LanguageId == languageId));
    }
    
    internal void AddTranslation(string languageId, string messageId)
    {
        Translations ??= new();

        if (!HasTranslation(languageId))
            Translations.Add(new(languageId, messageId));
    }

    internal void AddCharge(decimal cost, string description)
    {
        Charge += cost;
        Broadcast(new CostIncurred(this, description, cost));
    }
}

public class SystemMessage : Message
{
    public SystemMessage(string text) : base("system", Users.User.System, text)
    {
        Language = "en";
    }
}
