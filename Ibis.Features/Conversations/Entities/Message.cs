using Sparc.Core;

namespace Ibis.Features.Conversations.Entities;

public class Message : Root<string>
{
    public string ConversationId { get; private set; }
    public string UserId { get; private set; }
    public string Language { get; private set; }
    public SourceTypes SourceType { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? Text { get; private set; }
    public string? AudioId { get; private set; }
    public List<Translation> Translations { get; private set; }
    public bool IsNew { get; set; }
    public string UserName { get; set; }
    public string UserInitials { get; set; }

    protected Message()
    {
        Id = Guid.NewGuid().ToString();
        ConversationId = "";
        UserId = "";
        Language = "";
        SourceType = SourceTypes.Text;
        Translations = new();
        UserName = "";
        UserInitials = "";
    }

    public Message(string conversationId, string fromUserId, string language, SourceTypes sourceType, string userName, string initials) : this()
    {
        ConversationId = conversationId;
        UserId = fromUserId;
        Language = language;
        SourceType = sourceType;
        Timestamp = DateTime.UtcNow;
        UserName = userName;
        UserInitials = initials;
    }

    public void SetText(string text) => Text = text;
    public void SetAudio(string audioId) => AudioId = audioId;

    public bool HasTranslation(string language)
    {
        return Translations.Any(x => x.Language.StartsWith(language));
    }
    public void AddTranslation(string language, string result, double? score = 0)
    {
        Translation translation = new(language);
        translation.SetText(result);

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
}
