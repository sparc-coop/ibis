using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Ibis.Features._Plugins;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class IbisHub : SparcHub
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
        if (Context.UserIdentifier != null)
            await Users.ExecuteAsync(Context.UserIdentifier, u => u.GoOnline(Context.ConnectionId));
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.UserIdentifier != null)
            await Users.ExecuteAsync(Context.UserIdentifier, u => u.GoOffline());

        await base.OnDisconnectedAsync(exception);
    }

    public async Task ReceiveAudio(IAsyncEnumerable<byte[]> audio)
    {
        var sessionId = await Listener.BeginListeningAsync();

        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

        await foreach (var chunk in audio)
            await Listener.ListenAsync(sessionId, chunk);
    }
}
