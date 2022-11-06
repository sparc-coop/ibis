namespace Ibis.Features._Plugins;

public class AzureTranslator : ITranslator
{
    readonly HttpClient Client;

    public AzureTranslator(IConfiguration configuration)
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri("https://api.cognitive.microsofttranslator.com"),
        };
        Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", configuration.GetConnectionString("Cognitive"));
        Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", "southcentralus");
    }

    public async Task<List<Message>> TranslateAsync(Message message, string fromLanguageId, List<Language> toLanguages)
    {
        object[] body = new object[] { new { message.Text } };
        var from = $"&from={fromLanguageId.Split('-').First()}";
        var to = "&to=" + string.Join("&to=", toLanguages.Select(x => x.Id.Split('-').First()));

        var result = await Client.PostAsJsonAsync<object[], TranslationResult[]>($"/translate?api-version=3.0{from}{to}", body);
        var translatedMessages = new List<Message>();

        if (result != null && result.Length > 0)
        {
            foreach (TranslationResult o in result)
                foreach (Translation t in o.Translations)
                {
                    var translatedMessage = new Message(message, t.To, t.Text);
                    translatedMessages.Add(translatedMessage);

                    var cost = message.Text!.Length / 1_000_000M * -10.00M; // $10 per 1M characters
                    message.AddCharge(cost, $"Translate message from {message.User.Name} from {message.Language} to {t.To}");
                }
        }

        return translatedMessages;
    }

    public async Task<List<Message>> TranslateAsync(Message message, List<Language> toLanguages)
    {
        return await TranslateAsync(message, message.Language, toLanguages);
    }

    public async Task<List<Language>> GetLanguagesAsync()
    {
        var result = await Client.GetFromJsonAsync<LanguageList>("/languages?api-version=3.0&scope=translation");

        return result!.translation
            .Select(x => new Language(x.Key, x.Value.name, x.Value.nativeName, x.Value.dir == "rtl"))
            .ToList();
    }

    public async Task<Language?> GetLanguageAsync(string language)
    {
        var languages = await GetLanguagesAsync();
        return languages.FirstOrDefault(x => x.Id == language);
    }
}

public record TranslationResult(DetectedLanguage DetectedLanguage, TextResult SourceText, Translation[] Translations);
public record DetectedLanguage(string Language, float Score);
public record TextResult(string Text, string Script);
public record Translation(string Text, TextResult Transliteration, string To, Alignment Alignment, SentenceLength SentLen);
public record Alignment(string Proj);
public record SentenceLength(int[] SrcSentLen, int[] TransSentLen);
public record LanguageList(Dictionary<string, LanguageItem> translation);//dictionary of languages //List<LanguageItem>> translation);//
public record LanguageItem(string name, string nativeName, string dir, List<Dialect>? Dialects);