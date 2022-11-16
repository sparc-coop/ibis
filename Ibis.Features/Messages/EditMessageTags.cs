namespace Ibis.Features.Messages;

public record EditMessageTagsRequest(string RoomId, string MessageId, List<MessageTag> Tags);
public class EditMessageTags : Feature<EditMessageTagsRequest, List<MessageTag>>
{
    public EditMessageTags(IRepository<Message> messages, IRepository<User> users)
    {
        Messages = messages;
        Users = users;
    }

    public IRepository<Message> Messages { get; }
    public IRepository<User> Users { get; }

    public override async Task<List<MessageTag>> ExecuteAsync(EditMessageTagsRequest request)
    {
        var isMessageId = Guid.TryParse(request.MessageId, out _);

        var message = isMessageId
                ? Messages.Query.FirstOrDefault(x => x.RoomId == request.RoomId && x.Id == request.MessageId)
                : Messages.Query.FirstOrDefault(x => x.RoomId == request.RoomId && x.Tag == request.MessageId && x.User.Id == User.Id());
        
        if (message == null)
            throw new NotFoundException($"Message ID {request.MessageId} does not exist!");

        message.SetTags(request.Tags);
        await Messages.UpdateAsync(message);

        return message.Tags;
    }
}
