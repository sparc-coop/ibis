namespace Ibis.Features.Messages;

public record DeleteMessageRequest(string RoomId, string MessageId);
public class DeleteMessage : Feature<DeleteMessageRequest, Room>
{
    public IRepository<Room> Rooms { get; }
    public IRepository<Message> Messages { get; }

    public DeleteMessage(IRepository<Room> rooms, IRepository<Message> messages)
    {
        Rooms = rooms;
        Messages = messages;
    }

    public override async Task<Room> ExecuteAsync(DeleteMessageRequest request)
    {
        var message = await Messages.FindAsync(request.MessageId);
        await Messages.DeleteAsync(message);
        return await Rooms.FindAsync(request.RoomId);
    }
}
