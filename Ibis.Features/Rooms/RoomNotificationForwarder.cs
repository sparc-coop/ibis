using Microsoft.AspNetCore.SignalR;
using Sparc.Realtime;

namespace Ibis.Features.Rooms
{
    public class RoomNotificationForwarder : BackgroundFeature<RoomNotification>
    {
        public RoomNotificationForwarder(IHubContext<RoomHub> hub)
        {
            Hub = hub;
        }

        public IHubContext<RoomHub> Hub { get; }

        public override async Task ExecuteAsync(RoomNotification item)
        {
            await Hub.Clients.Group(item.RoomId).SendAsync(item.GetType().Name, item);
        }
    }
}
