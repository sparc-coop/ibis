using Sparc.Blossom;
using Sparc.Data;

namespace Ibis.Features.Messages;

public record LanguageAdded(string RoomId, Language Language) : Notification(RoomId);
public class TranslateExistingMessages : RealtimeFeature<LanguageAdded>
{
    public TranslateExistingMessages(IRepository<Room> rooms, IRepository<Message> messages, ITranslator translator)
    {
        Rooms = rooms;
        Messages = messages;
        Translator = translator;
    }

    public IRepository<Room> Rooms { get; }
    public IRepository<Message> Messages { get; }
    public ITranslator Translator { get; }

    public override async Task ExecuteAsync(LanguageAdded notification)
    {
        var room = await Rooms.FindAsync(notification.RoomId);
        if (room == null)
            throw new NotFoundException("Room not found!");
        
        var messages = await Messages.Query
            .Where(x => x.RoomId == notification.RoomId && x.SourceMessageId == null)
            .ToListAsync();

        foreach (var message in messages)
        {
            var translatedMessages = await room.TranslateAsync(message, Translator);

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
