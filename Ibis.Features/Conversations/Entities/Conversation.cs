using Sparc.Core;
using System.Globalization;

namespace Ibis.Features.Conversations.Entities;

public class Conversation : Root<string>
{
    public string ConversationId { get; set; }
    public string Name { get; set; }
    public string HostUserId { get; set; }
    public List<Language> Languages { get; private set; }

    private Conversation() 
    { 
        Id = Guid.NewGuid().ToString();
        ConversationId = Id;
        Name = "New Conversation";
        HostUserId = "";
        Languages = new();
    }

    public Conversation(string name, string hostUserId) : this()
    {
        Name = name;
        HostUserId = hostUserId;
    }

    public void AddLanguage(string language)
    {
        if (Languages.Any(x => x.Name == language))
            return;

        Languages.Add(new(language));
    }
}
