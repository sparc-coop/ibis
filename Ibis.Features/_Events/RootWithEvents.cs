namespace Sparc.Core;

public interface IRootWithEvents
{
    public List<INotification>? Events { get; }
}

public class RootWithEvents<T> : Root<T>, IRootWithEvents where T : notnull
{
    private List<INotification>? _events;
    public List<INotification>? Events => _events;

    public void Broadcast(INotification notification)
    {
        _events ??= new List<INotification>();
        _events.Add(notification);
    }
}