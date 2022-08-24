namespace Ibis.Features.Messages;

public interface ISynthesizer
{
    Task<string?> SpeakAsync(Message message);
    Task<List<Voice>> GetVoicesAsync(string? language = null, string? dialect = null, string? gender = null);
}