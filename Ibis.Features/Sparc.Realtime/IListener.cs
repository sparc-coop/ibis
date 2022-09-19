namespace Ibis.Features.Sparc.Realtime;

public interface IListener
{
    Task<string> BeginListeningAsync();
    Task ListenAsync(string sessionId, byte[] audioChunk);
}