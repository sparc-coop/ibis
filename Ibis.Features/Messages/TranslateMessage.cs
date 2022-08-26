namespace Ibis.Features.Messages;

public class TranslateMessage : BackgroundFeature<MessageTextChanged>
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
        var room = await Rooms.FindAsync(notification.RoomId);
        var message = await Messages.FindAsync(notification.MessageId);

        if (room == null || message == null)
            throw new NotFoundException("Not found!");
        
        var translatedMessages = await room.TranslateAsync(message, Translator);

        foreach (var translatedMessage in translatedMessages)
        {
            await Messages.AddAsync(translatedMessage);
        }
        
        await Messages.UpdateAsync(message);
    }
}
