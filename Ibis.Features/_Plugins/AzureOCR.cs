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

                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();

                string allContent = GetAllContentsConcatenated(responseContent);
                return allContent;
            }
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

        public string GetAllContentsConcatenated(string jsonResponse)
        {
            var jsonObject = JObject.Parse(jsonResponse);

            StringBuilder allContents = new StringBuilder();

            // Iterate through each page
            var pages = jsonObject["readResult"]["pages"] as JArray;
            if (pages != null)
            {
                foreach (var page in pages)
                {
                    var words = page["words"] as JArray;
                    if (words != null)
                    {
                        // Concatenate each word's content
                        foreach (var word in words)
                        {
                            allContents.Append(word["content"].ToString());
                            allContents.Append(" "); // Adding space for readability
                        }
                    }
                }
            }

            return allContents.ToString().Trim(); // Trim to remove any extra space at the end
        }
    }
}
