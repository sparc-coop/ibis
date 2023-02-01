namespace Ibis.Messages;

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
        if (notification.Message.SourceMessageId != null) // Message already translated
            return;

        var room = await Rooms.FindAsync(notification.Message.RoomId);

        if (room == null || notification.Message == null)
            throw new NotFoundException("Not found!");

        var translatedMessages = await room.TranslateAsync(notification.Message, Translator, true);

        foreach (var translatedMessage in translatedMessages)
            await Messages.UpdateAsync(translatedMessage);
        
        await Messages.UpdateAsync(notification.Message);
    }
}
