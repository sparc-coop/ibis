namespace Ibis.Features;

public interface ISpeaker
{
    Task<AudioMessage?> SpeakAsync(Message message);
    Task<List<Voice>> GetVoicesAsync(string? language = null, string? dialect = null, string? gender = null);
    Task<AudioMessage> SpeakAsync(List<Message> messages);
}