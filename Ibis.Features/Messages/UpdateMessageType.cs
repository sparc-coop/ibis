namespace Ibis.Features.Messages;

public record UpdateMessageTypeRequest(string messageId, string roomId, string type);
public class UpdateMessageType : Feature<UpdateMessageTypeRequest, Message>
{
    public UpdateMessageType(IRepository<Message> messages)
    {
        Messages = messages;
    }

    public IRepository<Message> Messages { get; }
    public IRepository<User> Users { get; }

    public override async Task<Message> ExecuteAsync(UpdateMessageTypeRequest request)
    {
        var isMessageId = Guid.TryParse(request.messageId, out _);

        var message = isMessageId
                ? Messages.Query.FirstOrDefault(x => x.RoomId == request.roomId && x.Id == request.messageId)
                : Messages.Query.FirstOrDefault(x => x.RoomId == request.roomId && x.Tag == request.messageId && x.User.Id == User.Id());

        if (message == null)
            throw new NotFoundException($"Message ID {request.messageId} does not exist!");

        message.Type = request.type;
        await Messages.UpdateAsync(message);

        return message;
    }
}
