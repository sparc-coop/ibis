using Ibis.Features.Conversations.Entities;
using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Conversations
{
    public class GetConversation : PublicFeature<string, Conversation>
    {
        public GetConversation(IRepository<Conversation> conversations)
        {
            Conversations = conversations;
        }

        public IRepository<Conversation> Conversations { get; }

        public async override Task<Conversation> ExecuteAsync(string id)
        {
            var conversation = await Conversations.FindAsync(id);
            if (conversation == null)
                throw new NotFoundException($"Conversation {id} not found!");

            return conversation;
        }
    }
}
