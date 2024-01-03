namespace Ibis;

public interface IListener
{
    Task<string> BeginListeningAsync(Dialect? dialect);
    Task ListenAsync(string sessionId, byte[] audioChunk) => Task.CompletedTask;
}