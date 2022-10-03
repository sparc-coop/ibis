using Ibis.Features.Sparc.Realtime;

namespace Ibis.Features.Messages;

public class TranslateMessage : RealtimeFeature<MessageTextChanged>
{
    public TranslateMessage(IRepository<Message> messages, IRepository<Room> rooms, ITranslator translator)
    {
        Messages = messages;
        Rooms = rooms;
        Translator = translator;
    }

    public IRepository<Message> Messages { get; }
    public IRepository<Room> Rooms { get; }
    public ITranslator Translator { get; }

    public override async Task ExecuteAsync(MessageTextChanged notification)
    {
        var room = await Rooms.FindAsync(notification.GroupId);

        if (room == null || notification.Message == null)
            throw new NotFoundException("Not found!");

        if (notification.Message.SourceMessageId != null) // Message already translated
            return;
        
        var translatedMessages = await room.TranslateAsync(notification.Message, Translator);

        foreach (var translatedMessage in translatedMessages)
            await Messages.AddAsync(translatedMessage);
        
        await Messages.UpdateAsync(notification.Message);
    }
}
