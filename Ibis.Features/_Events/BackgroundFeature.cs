using MediatR;

namespace Sparc.Features;

public interface INotification : MediatR.INotification
{ }

public abstract class BackgroundFeature<T> : INotificationHandler<T> where T : INotification
{
    public abstract Task ExecuteAsync(T item);

    public async Task Handle(T request, CancellationToken cancellationToken)
    {
        await ExecuteAsync(request);
    }
}

