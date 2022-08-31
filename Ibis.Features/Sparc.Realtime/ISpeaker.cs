namespace Ibis.Features.Sparc.Realtime;

public interface ISpeaker
{
    Task<AudioMessage?> SpeakAsync(Message message);
    Task<List<Voice>> GetVoicesAsync(string? language = null, string? dialect = null, string? gender = null);
}