using Sparc.Core;

namespace Ibis.Features.Conversations.Entities;

public class Conversation : Root<string>
{
    public string ConversationId { get; }
    public string Name { get; }
    public string HostUserId { get; }

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
