namespace Ibis.Features.Messages;

public class PrivateMessage : Message
{
    public string ToUserId { get; private set; }

    private PrivateMessage() : base()
    {
        ToUserId = "";
    }

    public PrivateMessage(string roomId, string fromUserId, string toUserId, string language, SourceTypes sourceType, string name, string initials)
        : base(roomId, fromUserId, language, sourceType, name, initials)
    {
        ToUserId = toUserId;
    }
}
