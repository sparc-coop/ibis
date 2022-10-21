using EllipticCurve.Utils;
using Ibis.Features.Sparc.Realtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Threading.Channels;

namespace Ibis.Features._Plugins;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class IbisHub : SparcHub
{
    public IRepository<User> Users { get; }
    public IRepository<Room> Rooms { get; }

    public IbisHub(IRepository<User> users, IRepository<Room> rooms) : base()
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

    public async Task ReceiveAudio(string sessionId, byte[] audio)
    {
        using (var stream = new FileStream($"{sessionId}.wav", FileMode.Append))
        {
            stream.Write(audio, 0, audio.Length);
        }
        await AzureListener.ListenAsync(sessionId, audio);
    }
}
