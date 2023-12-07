using Azure;
using Azure.AI.TextAnalytics;

namespace Ibis._Plugins
{
    public class AzureLanguageDetector
    {
        readonly Uri endpoint = new Uri("https://ibis-cognitive.cognitiveservices.azure.com/");
        readonly string SubscriptionKey;

        public AzureLanguageDetector(IConfiguration configuration)
        {
            SubscriptionKey = configuration.GetConnectionString("Cognitive")!;
        }
        public string LanguageDetectionExample(TextAnalyticsClient client, string text)
        {
            Azure.AI.TextAnalytics.DetectedLanguage detectedLanguage = client.DetectLanguage(text);
            //Console.WriteLine("Language:");
            //Console.WriteLine($"\t{detectedLanguage.Name},\tISO-6391: {detectedLanguage.Iso6391Name}\n");
            return detectedLanguage.Name;
        }

        public string CallAzureLanguageDetector(string text)
        {
            var client = new TextAnalyticsClient(endpoint, new AzureKeyCredential(SubscriptionKey));
            var result = LanguageDetectionExample(client, text);

            return result;
        }
    }
}
