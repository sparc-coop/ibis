using Ibis.Features._Plugins;
using Microsoft.AspNetCore.SignalR;
using static System.Net.Mime.MediaTypeNames;

namespace Ibis.Features.Messages;

public record TypeMessageRequest(string RoomId, string Text);
public class TypeMessage : Feature<TypeMessageRequest, Message>
{
    public TypeMessage(IRepository<Message> messages, IRepository<User> users, BackgroundTaskQueue<Message> queue)
    {
        Messages = messages;
        Users = users;
        Queue = queue;
    }

    public IRepository<Message> Messages { get; }
    public IRepository<User> Users { get; }
    public IRepository<Room> Rooms { get; }
    public BackgroundTaskQueue<Message> Queue { get; }

    public override async Task<Message> ExecuteAsync(TypeMessageRequest request)
    {
        var user = await Users.FindAsync(User.Id());
        var message = new Message(request.RoomId, user!, request.Text);
        await Messages.AddAsync(message);

        await Queue.ProcessAsync(message);

        return message;
}
}
