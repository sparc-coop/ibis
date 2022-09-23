using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Ibis.Features.Rooms;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class IbisHub : SparcHub
{
    public IRepository<User> Users { get; }
    public IRepository<Room> Rooms { get; }

    public IbisHub(IRepository<User> users, IRepository<Room> rooms)
    {
        Users = users;
        Rooms = rooms;
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
}

