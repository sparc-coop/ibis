namespace Ibis.Features.Sparc.Realtime;

public interface IListener
{
    Task<string> BeginListeningAsync();
    static Task ListenAsync(string sessionId, byte[] audioChunk) => Task.CompletedTask;
}