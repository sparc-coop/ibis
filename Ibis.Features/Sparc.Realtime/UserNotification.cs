using Microsoft.AspNetCore.SignalR;

namespace Ibis.Features.Sparc.Realtime;

public record UserNotification(string UserId) : INotification;

public class UserNotificationForwarder : RealtimeFeature<UserNotification>
{
    public UserNotificationForwarder(IHubContext<RoomHub> hub)
    {
        Hub = hub;
    }

    public IHubContext<RoomHub> Hub { get; }

    public override async Task ExecuteAsync(UserNotification notification)
    {
        await Hub.Clients.Client(notification.UserId).SendAsync(notification.GetType().Name, notification);
    }
}