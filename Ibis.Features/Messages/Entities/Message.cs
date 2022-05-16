namespace Ibis.Features.Messages;

public class Message : Root<string>
{
    public string RoomId { get; private set; }
    public string UserId { get; private set; }
    public string Language { get; private set; }
    public SourceTypes SourceType { get; private set; }
    public DateTime Timestamp { get; private set; }
    public long? Duration { get; private set; }
    public string? Text { get; private set; }
    public string? ModifiedText { get; private set; }
    public string? AudioId { get; private set; }
    public string? ModifiedAudioId { get; private set; }
    public string? OriginalUploadFileName { get; set; }
    public List<Translation> Translations { get; private set; }
    public string UserName { get; set; }
    public string UserInitials { get; set; }
    public string? SubroomId { get; set; }
    public string? Color { get; set; }

    protected Message()
    {
        Id = Guid.NewGuid().ToString();
        RoomId = "";
        UserId = "";
        Language = "";
        SourceType = SourceTypes.Text;
        Translations = new();
        UserName = "";
        UserInitials = "";
    }

    public Message(string roomId, string fromUserId, string language, SourceTypes sourceType, string userName, string initials) : this()
    {
        RoomId = roomId;
        UserId = fromUserId;
        Language = language;
        SourceType = sourceType;
        Timestamp = DateTime.UtcNow;
        UserName = userName;
        UserInitials = initials;
    }

    public void SetText(string text) => Text = text;
    public void SetModifiedText(string text) => ModifiedText = text;
    public void SetAudio(string audioId) => AudioId = audioId;
    public void SetModifiedAudio(string audioId) => ModifiedAudioId = audioId;
    public void SetOriginalUploadFileName(string fileName) => OriginalUploadFileName = fileName;
    public void SetSubroomId(string id) => SubroomId = id;
    
    public bool HasTranslation(string language)
    {
        return Translations.Any(x => x.Language.StartsWith(language));
    }
    public void AddTranslation(string language, string result, double? score = 0)
    {
        Translation translation = new(language, result);

        var existing = Translations.FindIndex(x => x.Language == language);

        if (existing == -1)
            Translations.Add(translation);
        else
            Translations[existing] = translation;
    }

    internal string GetTranslation(string language)
    {
        return !HasTranslation(language)
            ? string.Empty
            : Translations.First(x => x.Language.StartsWith(language)).Text;
    }

    internal void SetTimestamp(long offsetInTicks, TimeSpan duration)
    {
        Timestamp = Timestamp.AddTicks(offsetInTicks);
        Duration = duration.Ticks;
    }
}
