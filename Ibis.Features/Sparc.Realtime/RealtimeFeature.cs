using Ardalis.ApiEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sparc.Realtime;

[RealtimeFuture]
public abstract class RealtimeFeature<T> : BaseAsyncEndpoint.WithRequest<T>.WithoutResponse, INotificationHandler<T> where T : SparcNotification
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract Task ExecuteAsync(T item);

    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task Handle(T request, CancellationToken cancellationToken)
    {
        await ExecuteAsync(request);
    }

    [HttpPost("")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public override async Task<ActionResult> HandleAsync(T request, CancellationToken cancellationToken = default)
    {
        await Handle(request, cancellationToken);
        return Ok();
    }
}

public class RealtimeFutureAttribute : RouteAttribute
{
    public RealtimeFutureAttribute() : base($"events/[controller]")
    {
    }
}
