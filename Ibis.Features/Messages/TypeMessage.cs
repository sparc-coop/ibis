namespace Ibis.Features.Messages;

public record TypeMessageRequest(string RoomId, string Text);
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
        var user = await Users.FindAsync(User.Id());
        var message = new Message(request.RoomId, user!, request.Text);
        await Messages.AddAsync(message);

        return message;
}
}
