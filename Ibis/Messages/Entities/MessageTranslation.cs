namespace Ibis.Messages;

public class MessageTranslation(string languageId, string messageId)
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string LanguageId { get; set; } = languageId;
    public string SourceMessageId { get; set; } = messageId;

    internal virtual Message? SourceMessage { get; set; }
}
