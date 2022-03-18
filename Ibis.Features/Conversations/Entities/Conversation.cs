using Sparc.Core;

namespace Ibis.Features.Conversations.Entities;

public class Conversation : Root<string>
{
    public string ConversationId { get; set; }
    public string Name { get; set; }
    public string HostUserId { get; set; }

    private Conversation() 
    { 
        Id = Guid.NewGuid().ToString();
        ConversationId = Id;
        Name = "New Conversation";
        HostUserId = "";
    }

    public Conversation(string name, string hostUserId) : this()
    {
        Name = name;
        HostUserId = hostUserId;
    }
}
