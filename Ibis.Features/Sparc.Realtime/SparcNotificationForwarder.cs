using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Ibis.Features.Sparc.Realtime;

public class SparcNotificationForwarder<TNotification> : RealtimeFeature<TNotification> where TNotification : SparcNotification
{
    public SparcNotificationForwarder(IHubContext<IbisHub> hub)
    {
        Hub = hub;
    }

    public IHubContext<IbisHub> Hub { get; }

    public override async Task ExecuteAsync(TNotification notification)
    {
        if (notification.GroupId != null)
            await Hub.Clients.Group(notification.GroupId).SendAsync(notification.GetType().Name, notification);
        else if (notification.UserId != null)
            await Hub.Clients.Client(notification.UserId).SendAsync(notification.GetType().Name, notification);
    }
}


