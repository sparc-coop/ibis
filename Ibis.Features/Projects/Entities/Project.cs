using Newtonsoft.Json;
using Sparc.Core;

namespace Ibis.Features;

public class Project : Root<string>
{
    public Project()
    {

    }
    public Project(string userId, 
        string type, 
        string name,
        string url = null, 
        string size = null, 
        string format = null, 
        string duration = null,
        string language = null,
        string status = null) : base(userId)
    {
        Id = Guid.NewGuid().ToString();
        UserId = userId;
        Type = type;
        Name = name;
        Url = url;
        Size = size;
        Format = format;
        Duration = duration;
        OriginLanguage = language;
        RegisterDate = DateTime.UtcNow;
        Status = string.IsNullOrEmpty(status) ? "Processing" : status;
    }

    [JsonProperty("id")]
    public override string Id { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Url { get; set; }
    public string Size { get; set; }
    public string Format { get; set; }
    public string Duration { get; set; }
    public string OriginLanguage { get; set; }
    public DateTime RegisterDate { get; set; }
    public string Status { get; set; }
}
