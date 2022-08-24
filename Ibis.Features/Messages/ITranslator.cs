namespace Ibis.Features.Messages;

public interface ITranslator
{
    Task<List<Message>> TranslateAsync(Message message, params string[] toLanguages);
    Task<List<Language>> GetLanguagesAsync();
}