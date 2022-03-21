using Ibis.Features.Conversations.Entities;
using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Conversations
{
    public class StartConversation : Feature<Conversation>
    {
        public StartConversation(IRepository<Conversation> conversations)
        {
            Conversations = conversations;
        }

        public IRepository<Conversation> Conversations { get; }

        public async override Task<Conversation> ExecuteAsync()
        {
            var conversation = new Conversation("Test Conversation", User.Id());
            await Conversations.AddAsync(conversation);
            return conversation;
        }
    }
}
