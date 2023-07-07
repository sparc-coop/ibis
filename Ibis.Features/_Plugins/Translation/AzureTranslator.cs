namespace Ibis._Plugins.Translation;

public class AzureTranslator : ITranslator
{
    readonly HttpClient Client;

    public static LanguageList? Languages;

    public AzureTranslator(IConfiguration configuration)
    {
        Client = new HttpClient
        {
            BaseAddress = new Uri("https://api.cognitive.microsofttranslator.com"),
        };
        Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", configuration.GetConnectionString("Cognitive"));
        Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", "southcentralus");
    }

    public async Task<List<Message>> TranslateAsync(Message message, List<Language> toLanguages)
    {
        var translatedMessages = new List<Message>();

        // Split the translations into 10 max per call
        var batches = Batch(toLanguages, 10);
        foreach (var batch in batches)
        {
            object[] body = new object[] { new { message.Text } };
            List<string> translatedTagKeys = new();
            foreach (var tag in message.Tags.Where(x => x.Translate))
            {
                translatedTagKeys.Add(tag.Key);
                body = body.Append(new { Text = tag.Value }).ToArray();
            }

            var languageDictionary = batch.ToDictionary(x => x.Id.Split('-').First(), x => x);

            var from = $"&from={message.Language.Split('-').First()}";
            var to = "&to=" + string.Join("&to=", languageDictionary.Keys);

            var result = await Client.PostAsJsonAsync<object[], TranslationResult[]>($"/translate?api-version=3.0{from}{to}", body);

            if (result != null && result.Length > 0)
            {
                var translatedText = result.First();
                var translatedTags = result.Skip(1).ToList();

                foreach (Translation t in translatedText.Translations)
                {
                    // Zip up the message tag translations
                    var translatedMessageTags = translatedTagKeys
                        .Where(key => translatedTags[translatedTagKeys.IndexOf(key)].Translations.Any(x => x.To == t.To))
                        .Select(key => new MessageTag(key, translatedTags[translatedTagKeys.IndexOf(key)].Translations.First(x => x.To == t.To).Text, false))
                        .ToList();

                    var translatedMessage = new Message(message, languageDictionary[t.To], t.Text, translatedMessageTags);

                    translatedMessages.Add(translatedMessage);

                    var cost = message.Text!.Length / 1_000_000M * -10.00M; // $10 per 1M characters
                    message.AddCharge(0, cost, $"Translate message from {message.User.Name} from {message.Language} to {t.To}");
                }
            }
        }

        return translatedMessages;
    }

    public async Task<List<Language>> GetLanguagesAsync()
    {
        Languages ??= await Client.GetFromJsonAsync<LanguageList>("/languages?api-version=3.0&scope=translation");

        return Languages!.translation
            .Select(x => new Language(x.Key, x.Value.name, x.Value.nativeName, x.Value.dir == "rtl"))
            .ToList();
    }

    // from https://stackoverflow.com/a/13731854
    public static IEnumerable<IEnumerable<T>> Batch<T>(IEnumerable<T> items,
                                                       int maxItems)
    {
        return items.Select((item, inx) => new { item, inx })
                    .GroupBy(x => x.inx / maxItems)
                    .Select(g => g.Select(x => x.item));
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