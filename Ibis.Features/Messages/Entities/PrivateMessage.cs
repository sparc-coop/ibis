namespace Ibis.Features.Messages;

public class PrivateMessage : Message
{
    public string ToUserId { get; private set; }

    private PrivateMessage() : base()
    {
        ToUserId = "";
    }
}
