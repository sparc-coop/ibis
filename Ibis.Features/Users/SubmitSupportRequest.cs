using Markdig.Parsers;
using Microsoft.Azure.Cosmos.Spatial;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Ibis.Users
{

    public record SupportRequest(string? UserId, string Message, string? contactEmail);
    public class SubmitSupportRequest : Feature<SupportRequest, bool>
    {
        public IRepository<User> Users { get; }
        private readonly IConfiguration _config;
        public SubmitSupportRequest(IRepository<User> users, IConfiguration config)
        {
            Users = users;
            _config = config;
        }

        public override async Task<bool> ExecuteAsync(SupportRequest request)
        {

            List<Block> blocks = new List<Block>();

            Block header = new Block()
            {
                Type = "header",
                Text = new Text { Type = "plain_text", TextContent = "Ibis Support Request"}
            };
            blocks.Add(header); 

            if (request.UserId != null)
            {
                var user = await Users.GetAsync(User);

                Block userInfo = new Block()
                {
                    Type = "section",
                    Text = new Text { Type = "mrkdwn", TextContent = "Submitted by *" + user.UserName.ToLower() + "* (" + user.Id + ")" }
                 };
                blocks.Add(userInfo);
            }


            if (request.contactEmail != null)
            {
                Block email = new Block()
                {
                    Type = "section",
                    Text = new Text { Type = "mrkdwn", TextContent = "Contact Email: " + request.contactEmail }
                };
                blocks.Add(email);
            }


            Block supportInfo = new Block()
            {
                Type = "section",
                Text = new Text { Type = "mrkdwn", TextContent = "> " + request.Message }
            };
            blocks.Add(supportInfo);

            Payload payload = new Payload()
            {
                Channel = "C04NB9J31KJ",
                Text = request.Message,
                Blocks = blocks
            };

            var stringPayload = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://slack.com/api/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config["SlackSupportToken"]);

                //send channel message api.slack.com/methods/chat.postMessage
                HttpResponseMessage response = await client.PostAsync("chat.postMessage", httpContent);
                if (response.IsSuccessStatusCode)
                {
                    string responseText = await response.Content.ReadAsStringAsync();
                    return true;
                }
                else
                {
                    Console.WriteLine("Error");
                    return false;
                }
            }
        }

    }

}

public class Payload
{
    [JsonProperty("channel")]
    public string? Channel { get; set; }

    [JsonProperty("text")]
    public string? Text { get; set; }
    [JsonProperty("blocks")]
    //public Block[]? Blocks { get; set; }
    public List<Block>? Blocks { get; set; }
}

public class Block
{
    [JsonProperty("type")]
    public string? Type { get; set; }
    [JsonProperty("text")]
    public Text? Text { get; set; }
}

public class Text
{
    [JsonProperty("type")] 
    public string? Type { get; set; }

    [JsonProperty("text")]
    public string? TextContent { get; set; }
}
