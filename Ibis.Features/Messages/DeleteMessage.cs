namespace Ibis.Features.Messages;

public record DeleteMessageRequest(string RoomId, string MessageId);
public class DeleteMessage : Feature<DeleteMessageRequest, bool>
{
    public IRepository<Room> Rooms { get; }
    public IRepository<Message> Messages { get; }

    public DeleteMessage(IRepository<Room> rooms, IRepository<Message> messages)
    {
        Rooms = rooms;
        Messages = messages;
    }

    public override async Task<bool> ExecuteAsync(DeleteMessageRequest request)
    {
        var message = await Messages.FindAsync(request.MessageId);
        if (message == null || message.User.Id != User.Id()) 
            return false;

        await Messages.DeleteAsync(message);
        return true;
    }
}
