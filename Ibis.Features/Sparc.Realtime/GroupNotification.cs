using Microsoft.AspNetCore.SignalR;

namespace Ibis.Features.Sparc.Realtime;

public record GroupNotification(string GroupId) : SparcNotification(GroupId);
public class GroupNotificationForwarder : RealtimeFeature<GroupNotification>
{
    public GroupNotificationForwarder(IHubContext<SparcHub> hub)
    {
        Hub = hub;
    }

    public IHubContext<SparcHub> Hub { get; }

    public override async Task ExecuteAsync(GroupNotification notification)
    {
        await Hub.Clients.Group(notification.GroupId).SendAsync(notification.GetType().Name, notification);
    }
}


