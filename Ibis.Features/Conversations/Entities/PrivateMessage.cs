namespace Ibis.Features.Conversations.Entities;

public class PrivateMessage : Message
{
    public string ToUserId { get; private set; }

    private PrivateMessage() : base()
    {
        ToUserId = "";
    }

    public PrivateMessage(string conversationId, string fromUserId, string toUserId, string language, SourceTypes sourceType)
        : base(conversationId, fromUserId, language, sourceType)
    {
        ToUserId = toUserId;
    }
}
