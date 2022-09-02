using Microsoft.AspNetCore.SignalR;

namespace Ibis.Features.Sparc.Realtime;

public class GroupNotificationForwarder : RealtimeFeature<GroupNotification>
{
    public GroupNotificationForwarder(IHubContext<RoomHub> hub)
    {
        Hub = hub;
    }

    public IHubContext<RoomHub> Hub { get; }

    public override async Task ExecuteAsync(GroupNotification item)
    {
        await Hub.Clients.Group(item.GroupId).SendAsync(item.GetType().Name, item);
    }
}
