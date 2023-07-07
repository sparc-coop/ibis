using Ibis._Plugins.Translation;

namespace Ibis._Plugins.Speech;

public interface ISpeaker
{
    Task<AudioMessage?> SpeakAsync(Message message, string? voiceId = null);
    Task<List<Voice>> GetVoicesAsync(string? language = null, string? dialect = null, string? gender = null);
    Task<AudioMessage> SpeakAsync(List<Message> messages);
    Task<List<Language>> GetLanguagesAsync(ITranslator translator);
    Task<string?> GetClosestVoiceAsync(string language, string? gender, string deterministicId);
}