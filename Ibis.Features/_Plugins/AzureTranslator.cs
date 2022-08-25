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
        Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", configuration.GetConnectionString("Translator"));
        Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", "southcentralus");
    }

    public async Task<List<Message>> TranslateAsync(Message message, params string[] toLanguages)
    {
        object[] body = new object[] { new { message.Text } };
        var from = $"&from={message.Language.Split('-').First()}";
        var to = "&to=" + string.Join("&to=", toLanguages.Select(x => x.Split('-').First()));

        var result = await Client.PostAsJsonAsync<object[], TranslationResult[]>($"/translate?api-version=3.0{from}{to}", body);
        var translatedMessages = new List<Message>();

        if (result != null && result.Length > 0)
        {
            foreach (TranslationResult o in result)
                foreach (Translation t in o.Translations)
                {
                    translatedMessages.Add(new(message, t.To, t.Text));
                }
        }

        return translatedMessages;
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
