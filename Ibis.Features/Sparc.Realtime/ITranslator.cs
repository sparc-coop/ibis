namespace Ibis.Features.Sparc.Realtime;

public interface ITranslator
{
    Task<List<Message>> TranslateAsync(Message message, List<Language> toLanguages);
    Task<List<Language>> GetLanguagesAsync();
    Task<Language?> GetLanguageAsync(string language);
    Task<List<Message>> TranslateAsync(Message message, string fromLanguageId, List<Language> toLanguages);
}