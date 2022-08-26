namespace Ibis.Features.Messages;

public interface ITranslator
{
    Task<List<Message>> TranslateAsync(Message message, List<Language> toLanguages);
    Task<List<Language>> GetLanguagesAsync();
    Task<Language?> GetLanguageAsync(string language);
}