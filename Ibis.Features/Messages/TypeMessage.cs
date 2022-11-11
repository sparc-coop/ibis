namespace Ibis.Features.Messages;

public record TypeMessageRequest(string RoomId, string Text, string? Tag = null, string? MessageId = null);
public class TypeMessage : Feature<TypeMessageRequest, Message>
{
    public TypeMessage(IRepository<Message> messages, IRepository<User> users)
    {
        Messages = messages;
        Users = users;
    }

    public IRepository<Message> Messages { get; }
    public IRepository<User> Users { get; }

    public override async Task<Message> ExecuteAsync(TypeMessageRequest request)
    {
        var user = await Users.GetAsync(User);
        return await ExecuteAsUserAsync(request, user!);
    }

    internal async Task<Message> ExecuteAsUserAsync(TypeMessageRequest request, User user)
    {
        if (request.Tag != null || request.MessageId != null)
        {
            // If a tag is passed in, edit the message if it exists
            var existingMessage =
                request.MessageId != null
                ? Messages.Query.FirstOrDefault(x => x.RoomId == request.RoomId && x.Id == request.MessageId)
                : Messages.Query.FirstOrDefault(x => x.RoomId == request.RoomId && x.Tag == request.Tag && x.User.Id == user.Id);
            
            if (existingMessage != null)
            {
                if (request.MessageId != null && existingMessage.User.Id != user.Id)
                    throw new NotAuthorizedException("You are not permitted to edit another user's message.");

                existingMessage.SetText(request.Text);
                await Messages.UpdateAsync(existingMessage);
                return existingMessage;
            }
        }

        var message = new Message(request.RoomId, user!, request.Text, request.Tag);
        await Messages.AddAsync(message);

        return message;
    }
}
