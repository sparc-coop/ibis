using Markdig;

namespace Ibis.Messages;

public record Word(long Offset, long Duration, string Text);
public record EditHistory(DateTime Timestamp, string Text);
public class Message : Entity<string>
{
    public string RoomId { get; private set; }
    public string? SourceMessageId { get; private set; }
    public string Language { get; private set; }
    public bool? LanguageIsRTL { get; private set; }
    public DateTime Timestamp { get; private set; }
    public DateTime? LastModified { get; private set; }
    public DateTime? DeletedDate { get; private set; }
    public UserAvatar User { get; private set; }
    public AudioMessage? Audio { get; private set; }
    public string? Text { get; private set; }
    public List<MessageTranslation> Translations { get; private set; }
    public long Charge { get; private set; }
    public decimal Cost { get; private set; }
    public string? Tag { get; set; }
    public List<MessageTag> Tags { get; set; }
    public List<EditHistory> EditHistory { get; private set; }
    public string Html => Markdown.ToHtml(Text ?? string.Empty);
    internal virtual Room Room { get; private set; } = null!;
    internal virtual Message? SourceMessage { get; private set; }

    protected Message()
    {
        Id = Guid.NewGuid().ToString();
        RoomId = "";
        User = new User().Avatar;
        Language = "";
        Translations = new();
        EditHistory = new();
        Tags = new();
    }

    public Message(string roomId, User user, string text, string? tag = null) : this()
    {
        RoomId = roomId;
        User = user.Avatar;
        Language = user.Avatar.Language ?? "";
        LanguageIsRTL = user.Avatar.LanguageIsRTL;
        Audio = user.Avatar.Voice == null ? null : new(null, 0, user.Avatar.Voice);
        Timestamp = DateTime.UtcNow;
        Tag = tag;
        SetText(text);
    }

    public Message(Message sourceMessage, Language toLanguage, string text, List<MessageTag> translatedTags) : this()
    {
        RoomId = sourceMessage.RoomId;
        SourceMessageId = sourceMessage.Id;
        User = new(sourceMessage.User);
        Audio = sourceMessage.Audio?.Voice == null ? null : new(null, 0, new(sourceMessage.Audio.Voice));
        Language = toLanguage.Id;
        LanguageIsRTL = toLanguage.IsRightToLeft;
        Timestamp = sourceMessage.Timestamp;
        Tag = sourceMessage.Tag;
        SetText(text);
        SetTags(sourceMessage.Tags);
        SetTags(translatedTags, false);
    }

    public void SetText(string text, User? user = null)
    {
        if (user != null && user.Id != User.Id)
            throw new InvalidOperationException("You are not permitted to edit another user's message.");

        if (Text == text)
            return;

        if (Text != null)
            EditHistory.Add(new(LastModified ?? Timestamp, Text));

        Text = text;
        LastModified = DateTime.UtcNow;

        Broadcast(new MessageTextChanged(this));
    }

    public void SetTags(List<MessageTag> tags, bool fullReplace = true)
    {
        var keys = tags.Select(x => x.Key).ToList();
        if (fullReplace)
            Tags.RemoveAll(x => !keys.Contains(x.Key));

        foreach (var tag in tags)
        {
            var existing = Tags.FirstOrDefault(x => x.Key == tag.Key);
            if (existing != null)
                existing.Value = tag.Value;
            else
                Tags.Add(new(tag.Key, tag.Value, SourceMessageId == null && tag.Translate));
        }

        if (tags.Any(x => x.Translate))
            Broadcast(new MessageTextChanged(this));
    }

    internal async Task<(string?, Message?)> TranslateAsync(ITranslator translator, string languageId)
    {
        if (HasTranslation(languageId))
            return (Translations.First(x => x.LanguageId == languageId).SourceMessageId, null);

        var language = await translator.GetLanguageAsync(languageId);
        var translatedMessage = (await translator.TranslateAsync(this, new List<Language> { language! })).FirstOrDefault();

        if (translatedMessage != null)
            AddTranslation(translatedMessage);

        return (translatedMessage?.Id, translatedMessage);
    }

    internal async Task<List<Message>> TranslateAsync(ITranslator translator, bool forceRetranslation = false)
    {
        var languagesToTranslate = forceRetranslation
            ? Room.Languages.Where(x => x.Id != Language).ToList()
            : Room.Languages.Where(x => !HasTranslation(x.Id)).ToList();

        if (!languagesToTranslate.Any())
            return new();

        var translatedMessages = await translator.TranslateAsync(this, languagesToTranslate);


        // Add reference to all the new translated messages
        foreach (var translatedMessage in translatedMessages)
            AddTranslation(translatedMessage);

        return translatedMessages;
    }

    internal async Task<AudioMessage?> SpeakAsync(ISpeaker engine, string? voiceId = null)
    {
        if (voiceId == null && (Audio?.Voice == null || !Audio.Voice.StartsWith(Language)))
        {
            voiceId = await engine.GetClosestVoiceAsync(Language, User.Gender, User.Id);
        }

        var audio = await engine.SpeakAsync(this, voiceId);

        if (audio != null)
        {
            Audio = audio;
            Broadcast(new MessageAudioChanged(this));
        }

        return audio;
    }

    internal bool HasTranslation(string languageId)
    {
        return Language == languageId
            || (Translations != null && Translations.Any(x => x.LanguageId == languageId));
    }

    internal void AddTranslation(Message translatedMessage)
    {
        if (HasTranslation(translatedMessage.Language))
        {
            // Set the newly translated message's ID to the existing translation so that it is updated in the repository
            var translation = Translations.FirstOrDefault(x => x.LanguageId == translatedMessage.Language);
            if (translation != null)
                translatedMessage.Id = translation.SourceMessageId;
        }
        else
        {
            Translations.Add(new(translatedMessage.Language, translatedMessage.Id));
        }
    }

    internal void AddCharge(long ticks, decimal cost, string description)
    {
        Charge += ticks;
        Cost -= cost;
        if (ticks > 0)
            Broadcast(new CostIncurred(this, description, ticks));
    }

    internal void Delete(User user)
    {
        if (User.Id != user.Id)
            throw new InvalidOperationException("Only the message author can delete a message.");

        foreach (var translation in Translations.Select(x => x.SourceMessage))
            translation?.Delete(user);

        DeletedDate = DateTime.UtcNow;
        Broadcast(new MessageDeleted(this));
    }

    public void ToText() => Text = $"{User?.Name} {Timestamp:MM/dd/yyyy hh:mm tt}: {Text}";
    
    internal void ToSubtitles(DateTime firstMessageTimestamp)
    {
        if (Audio == null)
            Text = string.Empty;

        var start = Timestamp - firstMessageTimestamp;
        var end = Timestamp.Add(new(Audio!.Duration)) - firstMessageTimestamp;

        Text = $"{start:hh\\:mm\\:ss\\,fff} --> {end:hh\\:mm\\:ss\\,fff}{Environment.NewLine}{Text}{Environment.NewLine}";
    }

    internal void ToBrailleAscii()
    {
        Text = BrailleConverter.Convert(Text);
    }
}
