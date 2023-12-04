using Azure;
using Azure.AI.TextAnalytics;

namespace Ibis._Plugins
{
    public class AzureLanguageDetector
    {
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential("68fa2318639d48d7bf7333dd22939ce6");
        private static readonly Uri endpoint = new Uri("https://ibis-cognitive.cognitiveservices.azure.com/");

        public string LanguageDetectionExample(TextAnalyticsClient client, string text)
        {
            Azure.AI.TextAnalytics.DetectedLanguage detectedLanguage = client.DetectLanguage(text);
            //Console.WriteLine("Language:");
            //Console.WriteLine($"\t{detectedLanguage.Name},\tISO-6391: {detectedLanguage.Iso6391Name}\n");
            return detectedLanguage.Name;
        }

        public string CallAzureLanguageDetector(string text)
        {
            var client = new TextAnalyticsClient(endpoint, credentials);
            var result = LanguageDetectionExample(client, text);

            return result;
        }
    }
}
