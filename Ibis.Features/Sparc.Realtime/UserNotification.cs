using Microsoft.AspNetCore.SignalR;

namespace Ibis.Features.Sparc.Realtime;

public record UserNotification(string UserId) : SparcNotification(UserId);

public class UserNotificationForwarder : RealtimeFeature<UserNotification>
{
    public UserNotificationForwarder(IHubContext<SparcHub> hub)
    {
        Hub = hub;
    }

    public IHubContext<SparcHub> Hub { get; }

    public override async Task ExecuteAsync(UserNotification notification)
    {
        await Hub.Clients.Client(notification.UserId).SendAsync(notification.GetType().Name, notification);
    }
}