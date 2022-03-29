using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Conversations
{
    public class GetUserConversations : Feature<string, List<Conversation>>
    {
        public GetUserConversations(IRepository<Conversation> conversations, IRepository<Message> messages, IbisEngine ibisEngine)
        {
            Conversations = conversations;
            Messages = messages;
            IbisEngine = ibisEngine;
        }

        public IRepository<Conversation> Conversations { get; }
        public IRepository<Message> Messages { get; }
        public IbisEngine IbisEngine { get; }

        public override async Task<List<Conversation>> ExecuteAsync(string userId)
        {
            return await Conversations.Query.Where(x => x.HostUserId == userId).ToListAsync(); // || x.ActiveUsers.Any(y => y.UserId == userId)
        }
    }
}
