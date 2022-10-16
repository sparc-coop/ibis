namespace Ibis.Features.Sparc.Realtime;

public record UserNotification(string UserId) : MediatR.INotification;
