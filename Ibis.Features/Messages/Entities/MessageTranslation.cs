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
