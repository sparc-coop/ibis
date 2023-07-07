using Ibis._Plugins.Speech;
using Microsoft.AspNetCore.Authorization;

namespace Ibis._Plugins;

[Authorize]
public class IbisHub : BlossomHub
{
    public IRepository<User> Users { get; }
    public IRepository<Room> Rooms { get; }
    public IListener Listener { get; }

    public IbisHub(IRepository<User> users, IRepository<Room> rooms, IListener listener) : base()
    {
        Users = users;
        Rooms = rooms;
        Listener = listener;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        
        if (Context.UserIdentifier != null)
            await Users.ExecuteAsync(Context.UserIdentifier, u => u.GoOnline(Context.ConnectionId));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.UserIdentifier != null)
            await Users.ExecuteAsync(Context.UserIdentifier, u => u.GoOffline());

        await base.OnDisconnectedAsync(exception);
    }

    public async Task ReceiveAudio(IAsyncEnumerable<byte[]> audio)
    {
        var user = await Users.FindAsync(Context.UserIdentifier!);
        var dialect = user?.Avatar.Dialect;
        if (dialect == null)
            return;

        var sessionId = await Listener.BeginListeningAsync(new(dialect));

        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

        await foreach (var chunk in audio)
            await Listener.ListenAsync(sessionId, chunk);
    }
}
