using Ibis.Features.Conversations.Entities;
using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Conversations
{
    public record GetConversationRequest(string ConversationId, string Language);
    public record GetConversationResponse(Conversation conversation, List<Message> messages);
    public class GetConversation : PublicFeature<GetConversationRequest, GetConversationResponse>
    {
        public GetConversation(IRepository<Conversation> conversations, IRepository<Message> messages)
        {
            Conversations = conversations;
            Messages = messages;
        }

        public IRepository<Conversation> Conversations { get; }
        public IRepository<Message> Messages { get; }

        public async override Task<GetConversationResponse> ExecuteAsync(GetConversationRequest request)
        {
            var conversation = await Conversations.FindAsync(request.ConversationId);
            if (conversation == null)
                throw new NotFoundException($"Conversation {request.ConversationId} not found!");

            var messages = Messages.Query
                .Where(x => x.ConversationId == conversation.Id)
                .Where(x => x.Language == request.Language)
                .OrderBy(x => x.Timestamp)
                .ToList();

            return new(conversation, messages);
        }
    }
}
