namespace Ibis.Features.Messages;

public class MessageTranslation
{
    public MessageTranslation(string languageId, string messageId)
    {
        Id = Guid.NewGuid().ToString();
        LanguageId = languageId;
        MessageId = messageId;
    }

    public string Id { get; set; }
    public string LanguageId { get; set; }
    public string MessageId { get; set; }
}

public record Word(long Offset, long Duration, string Text);
public record EditHistory(DateTime Timestamp, string Text);
public class Message : Root<string>
{
    public string RoomId { get; private set; }
    public string? SourceMessageId { get; private set; }
    public string Language { get; protected set; }
    public DateTime Timestamp { get; private set; }
    public DateTime? LastModified { get; private set; }
    public UserAvatar User { get; private set; }
    public AudioMessage? Audio { get; private set; }
    public string? Text { get; private set; }
    public List<MessageTranslation> Translations { get; private set; }
    public decimal Charge { get; private set; }
    public string? Tag { get; set; }
    public List<EditHistory> EditHistory { get; private set; }

    protected Message()
    {
        Id = Guid.NewGuid().ToString();
        RoomId = "";
        User = new User().Avatar;
        Language = "";
        Translations = new();
        EditHistory = new();
    }

    public Message(string roomId, User user, string text, string? tag = null) : this()
    {
        RoomId = roomId;
        User = user.Avatar;
        Language = user.Avatar.Language ?? "";
        Audio = user.Avatar.Voice == null ? null : new(null, 0, user.Avatar.Voice);
        Timestamp = DateTime.UtcNow;
        Tag = tag;
        SetText(text);
    }

    public Message(Message sourceMessage, string toLanguage, string text) : this()
    {
        RoomId = sourceMessage.RoomId;
        SourceMessageId = sourceMessage.Id;
        User = new(sourceMessage.User);
        Audio = sourceMessage.Audio?.Voice == null ? null : new(null, 0, new(sourceMessage.Audio.Voice));
        Language = toLanguage;
        Timestamp = DateTime.UtcNow;
        Tag = sourceMessage.Tag;
        SetText(text);
    }

    public void SetText(string text)
    {
        if (Text == text)
            return;

        if (Text != null)
            EditHistory.Add(new(LastModified ?? Timestamp, Text));

        Text = text;
        LastModified = DateTime.UtcNow;
        
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
        if (!HasTranslation(languageId))
            Translations.Add(new(languageId, messageId));
    }

    internal void AddCharge(decimal cost, string description)
    {
        Charge += cost;
        Broadcast(new CostIncurred(this, description, cost));
    }

    internal string Html()
    {
        if (string.IsNullOrWhiteSpace(Text))
            return string.Empty;

        var paragraphs = Text
            .Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
            .Where(x => !string.IsNullOrWhiteSpace(x));

        return string.Join("\r\n", paragraphs.Select(x => $"<p>{x}</p>"));
    }
}
