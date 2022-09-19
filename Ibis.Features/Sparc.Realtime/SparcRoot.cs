using Sparc.Realtime;

namespace Sparc.Core;

public interface ISparcRoot
{
    public List<INotification>? Events { get; }
}

public class SparcRoot<T> : Root<T>, ISparcRoot where T : notnull
{
    private List<INotification>? _events;
    public List<INotification>? Events => _events;

    public void Broadcast(INotification notification)
    {
        _events ??= new List<INotification>();
        _events.Add(notification);
    }
}