namespace Ibis._Plugins.Translation;

public interface ITranslator
{
    Task<List<Message>> TranslateAsync(Message message, List<Language> toLanguages);
    Task<List<Language>> GetLanguagesAsync();
    async Task<Language?> GetLanguageAsync(string language)
    {
        var languages = await GetLanguagesAsync();
        return languages.FirstOrDefault(x => x.Id == language);
    }

    async Task<string?> TranslateAsync(string text, string fromLanguage, string toLanguage)
    {
        var language = await GetLanguageAsync(toLanguage)
            ?? throw new ArgumentException($"Language {toLanguage} not found");

        var message = new Message("", User.System, text, fromLanguage);
        var result = await TranslateAsync(message, new() { language });
        return result?.FirstOrDefault()?.Text;
    }

    async Task<bool> CanTranslateAsync(string fromLanguage, string toLanguage)
    {
        var from = await GetLanguageAsync(fromLanguage);
        var to = await GetLanguageAsync(toLanguage);
        return from != null && to != null;
    }
}