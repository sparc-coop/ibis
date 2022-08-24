namespace Ibis.Features.Messages;

public class MessageTranslation
{
    public MessageTranslation(string language, string messageId)
    {
        Language = language;
        MessageId = messageId;
    }

    public string Language { get; private set; }
    public string MessageId { get; }
    public double? Score { get; private set; }
}
