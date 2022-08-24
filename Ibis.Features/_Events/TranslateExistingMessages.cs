namespace Ibis.Features._Events;

public record LanguageAdded(string RoomId, string Language) : INotification;
public class TranslateExistingMessages : BackgroundFeature<LanguageAdded>
{
    public TranslateExistingMessages(IRepository<Message> messages, ITranslator translator)
    {
        Messages = messages;
        Translator = translator;
    }

    public IRepository<Message> Messages { get; }
    public ITranslator Translator { get; }

    public override async Task ExecuteAsync(LanguageAdded notification)
    {
        var untranslatedMessages = await Messages.Query
            .Where(x => x.RoomId == notification.RoomId)
            .ToListAsync();

        foreach (var message in untranslatedMessages.Where(x => !x.HasTranslation(notification.Language)))
        {
            var translatedMessages = await Translator.TranslateAsync(message, notification.Language);
            
            foreach (var translatedMessage in translatedMessages)
                await Messages.AddAsync(translatedMessage);

            if (translatedMessages.Any())
            {
                message.AddTranslation(notification.Language, translatedMessages.First().Id);
                await Messages.UpdateAsync(message);
            }
        }
    }
}
