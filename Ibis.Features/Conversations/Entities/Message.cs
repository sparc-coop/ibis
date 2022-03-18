using Sparc.Core;

namespace Ibis.Features.Conversations.Entities;

public class Message : Root<string>
{
    public string ConversationId { get; private set; }
    public string UserId { get; private set; }
    public string Language { get; private set; }
    public SourceTypes SourceType { get; private set; }
    public string? SourceTypeId { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? Text { get; private set; }
    public string? AudioId { get; private set; }

    protected Message()
    {
        Id = Guid.NewGuid().ToString();
        ConversationId = "";
        UserId = "";
        Language = "";
        SourceType = SourceTypes.Text;
    }

    public Message(string conversationId, string fromUserId, string language, SourceTypes sourceType, string? sourceTypeId = null) : this()
    {
        ConversationId = conversationId;
        UserId = fromUserId;
        Language = language;
        SourceType = sourceType;
        SourceTypeId = sourceTypeId;
        Timestamp = DateTime.UtcNow;
    }

    public void SetText(string text) => Text = text;
    public void SetAudio(string audioId) => AudioId = audioId;    
}
