using System;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;
using static System.Net.WebRequestMethods;
using Azure;
using Newtonsoft.Json.Linq;

namespace Ibis._Plugins
{
    public class AzureOCR
    {
        readonly string SubscriptionKey;
        readonly Uri Endpoint = new Uri("https://ibis-cognitive.cognitiveservices.azure.com/");

        public AzureOCR(IConfiguration configuration)
        {
            SubscriptionKey = configuration.GetConnectionString("Cognitive")!;
        }
        // Updated to be an async method returning Task<string>
        public async Task<string> MakeRequest(string imgUrl)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);

            // Request parameters
            queryString["features"] = "read";
            //queryString["model-name"] = "{string}";
            queryString["language"] = "en";
            //queryString["smartcrops-aspect-ratios"] = "{string}";
            queryString["gender-neutral-caption"] = "False";
            var uri = $"{Endpoint}computervision/imageanalysis:analyze?api-version=2023-04-01-preview&" + queryString;

            // Request body
            byte[] byteData = await GetImageBytes(imgUrl);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var response = await client.PostAsync(uri, content);

                // Ensure the request was successful
                response.EnsureSuccessStatusCode();

                // Read the string content of the response
                string responseContent = await response.Content.ReadAsStringAsync();

                // You can now process the response content as needed, for example:
                // If the response is JSON, you might parse it with a JSON library like Newtonsoft.Json
                // var jsonResponse = JObject.Parse(responseContent);

                // TODO: Add your processing logic here

                string firstContent = GetFirstContent(responseContent);
                return firstContent;
            }

            // This should be replaced with actual processing of the response
        }

        public async Task<byte[]> GetImageBytes(string imageUrl)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync();
            }
        }

        public string GetFirstContent(string jsonResponse)
        {
            var jsonObject = JObject.Parse(jsonResponse);

            // Navigate through the JSON structure
            var pages = jsonObject["readResult"]["pages"] as JArray;
            if (pages != null && pages.Count > 0)
            {
                var words = pages[0]["words"] as JArray;
                if (words != null && words.Count > 0)
                {
                    var firstContent = words[0]["content"].ToString();
                    return firstContent;
                }
            }

            return "No content found";
        }
    }
}
