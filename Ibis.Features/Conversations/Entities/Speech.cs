using Sparc.Core;

namespace Ibis.Features.Conversations.Entities;

public class Speech : Root<string>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Text { get; set; }
    public SourceTypes SourceType { get; set; }
    public DateTime Timestamp { get; private set; }

    public Speech()
    {
        Id = Guid.NewGuid().ToString();
    }

    public Speech(string name, string text, SourceTypes sourcetype) : this()
    {
        Name = name;
        Text = text;
        SourceType = sourcetype;
    }
}

