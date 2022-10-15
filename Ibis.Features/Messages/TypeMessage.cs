using Microsoft.AspNetCore.Mvc;

namespace Ibis.Features.Messages;

public record TypeMessageRequest(string RoomId, string Text, string? Tag = null);
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
        if (request.Tag != null)
        {
            // If a tag is passed in, edit the message if it exists
            var existingMessage = Messages.Query.FirstOrDefault(x => x.RoomId == request.RoomId && x.Tag == request.Tag);
            if (existingMessage != null)
            {
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
