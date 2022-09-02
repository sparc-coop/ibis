using Microsoft.AspNetCore.SignalR;

namespace Ibis.Features.Sparc.Realtime;

public record GroupNotification(string GroupId) : INotification;
public class GroupNotificationForwarder : RealtimeFeature<GroupNotification>
{
    public GroupNotificationForwarder(IHubContext<RoomHub> hub)
    {
        Hub = hub;
    }

    public IHubContext<RoomHub> Hub { get; }

    public override async Task ExecuteAsync(GroupNotification notification)
    {
        await Hub.Clients.Group(notification.GroupId).SendAsync(notification.GetType().Name, notification);
    }
}


