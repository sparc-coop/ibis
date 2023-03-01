namespace Ibis._Plugins
{
    public class Translator
    {
        public static List<Language>? Languages;

        public Translator(IEnumerable<ITranslator> translators)
        {
            Translators = translators;
        }

        public IEnumerable<ITranslator> Translators { get; }

        public async Task<List<Language>> GetLanguagesAsync()
        {
            if (Languages == null)
            {
                Languages = new();
                foreach (var translator in Translators)
                {
                    var languages = await translator.GetLanguagesAsync();
                    Languages.AddRange(languages.Where(x => !Languages.Any(y => y.Id.ToUpper() == x.Id.ToUpper())));
                }
            }

            return Languages;
        }

        public async Task<Language?> GetLanguageAsync(string language)
        {
            var languages = await GetLanguagesAsync();
            return languages.FirstOrDefault(x => x.Id == language);
        }

        public async Task<List<Message>> TranslateAsync(Message message, List<Language> toLanguages)
        {
            var processedLanguages = new List<Language>(toLanguages);
            var messages = new List<Message>();
            foreach (var translator in Translators)
            {
                var languages = await translator.GetLanguagesAsync();
                if (!languages.Any(x => x.Id.ToUpper() == message.Language.ToUpper()))
                    continue;
                
                var languagesToTranslate = processedLanguages.Where(x => languages.Any(y => y.Id.ToUpper() == x.Id.ToUpper())).ToList();
                messages.AddRange(await translator.TranslateAsync(message, languagesToTranslate));
                processedLanguages.RemoveAll(x => languagesToTranslate.Any(y => y.Id.ToUpper() == x.Id.ToUpper()));
                if (!processedLanguages.Any())
                    break;
            }

            return messages;
        }

        public async Task<string?> TranslateAsync(string text, string fromLanguage, string toLanguage)
        {
            var language = await GetLanguageAsync(toLanguage)
                ?? throw new ArgumentException($"Language {toLanguage} not found");

            var message = new Message("", User.System, text, fromLanguage);
            var result = await TranslateAsync(message, new() { language });
            return result?.FirstOrDefault()?.Text;
        }

    }
}
