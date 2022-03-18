using Ibis.Features.Conversations.Entities;
using Newtonsoft.Json;
using System.Text;

namespace Ibis.Features._Plugins
{
    public class IbisEngine
    {
        HttpClient Client { get; set; }

        public IbisEngine(IConfiguration configuration)
        {
            Client = new HttpClient
            {
                BaseAddress = new Uri("https://api.cognitive.microsofttranslator.com"),
            };
            Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", configuration.GetConnectionString("Translator"));
            Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", "southcentralus");
        }

        internal async Task Translate(Message message, List<Language> languages)
        {
            await Translate(message, languages.Where(x => x.Name != message.Language).Select(x => x.Name).ToArray());
        }

        internal async Task Translate(Message message, params string[] languages)
        {
            object[] body = new object[] { new { message.Text } };
            var from = $"&from={message.Language.Split('-').First()}";
            var to = "&to=" + string.Join("&to=", languages.Select(x => x.Split('-').First()));

            var result = await Post<TranslationResult[]>($"/translate?api-version=3.0{from}{to}", body);

            foreach (TranslationResult o in result)
                foreach (Translation t in o.Translations)
                    message.AddTranslation(t.To, SourceTypes.TextTranslator, t.Text);
        }

        private async Task<T> Post<T>(string url, object model)
        {
            var response = await Client.PostAsync(url, Jsonify(model));
            return await UnJsonify<T>(response);
        }

        private StringContent Jsonify(object model)
        {
            var json = JsonConvert.SerializeObject(model);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private async Task<T> UnJsonify<T>(HttpResponseMessage response)
        {
            var result = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }

    public record TranslationResult(DetectedLanguage DetectedLanguage, TextResult SourceText, Translation[] Translations);

    public record DetectedLanguage(string Language, float Score);

    public record TextResult(string Text, string Script);

    public record Translation(string Text, TextResult Transliteration, string To, Alignment Alignment, SentenceLength SentLen);

    public record Alignment(string Proj);

    public record SentenceLength(int[] SrcSentLen, int[] TransSentLen);
}
