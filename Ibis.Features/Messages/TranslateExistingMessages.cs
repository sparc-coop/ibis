using Ibis.Features.Sparc.Realtime;

namespace Ibis.Features.Messages;

public record LanguageAdded(string RoomId, Language Language) : INotification;
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

        foreach (var message in untranslatedMessages.Where(x => !x.HasTranslation(notification.Language.Id)))
        {
            var translatedMessages = await Translator.TranslateAsync(message, new() { notification.Language });

            foreach (var translatedMessage in translatedMessages)
                await Messages.AddAsync(translatedMessage);

            if (translatedMessages.Any())
            {
                message.AddTranslation(notification.Language.Id, translatedMessages.First().Id);
                await Messages.UpdateAsync(message);
            }
        }
    }
}
