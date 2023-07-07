using DeepL;
using DeepL.Model;

namespace Ibis._Plugins;

public class DeepLTranslator : ITranslator
{
    readonly DeepL.Translator Client;

    public static SourceLanguage[]? Languages;

    public DeepLTranslator(IConfiguration configuration)
    {
        Client = new(configuration["DeepLApi"]!);
    }

    public async Task<List<Message>> TranslateAsync(Message message, List<Language> toLanguages)
    {
        var translatedMessages = new List<Message>();
        TextTranslateOptions options = new()
        {
            SentenceSplittingMode = SentenceSplittingMode.Off
        };

        // Split the translations into 10 max per call
        foreach (var language in toLanguages)
        {
            var toLanguage = language.Id.ToUpper() == "EN" ? "en-US" : language.Id;
            var result = await Client.TranslateTextAsync(message.Text!, message.Language, toLanguage, options);
            var translatedMessage = new Message(message, language, result.Text, new());
            translatedMessages.Add(translatedMessage);
            var cost = message.Text!.Length / 1_000_000M * -25.00M; // $25 per 1M characters
            message.AddCharge(0, cost, $"Translate message from {message.User.Name} from {message.Language} to {toLanguage}");
        }

        return translatedMessages;
    }

    public async Task<List<Language>> GetLanguagesAsync()
    {
        Languages ??= await Client.GetSourceLanguagesAsync();
        return Languages
            .Select(x => new Language(x.Code, x.Name, x.Name, false))
            .ToList();
    }
}