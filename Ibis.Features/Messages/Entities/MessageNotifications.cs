namespace Ibis.Messages;

public record Notification : INotification
{
    public Notification(string? subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public string? SubscriptionId { get; set; }
}

public record MessageAudioChanged(Message Message) : Notification(Message.RoomId + "|" + Message.Language);
public record MessageTextChanged(Message Message) : Notification(Message.RoomId + "|" + Message.Language);
public record MessageDeleted(Message Message) : Notification(Message.RoomId + "|" + Message.Language);
