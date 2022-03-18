using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Conversations
{
    public record GetConversationRequest(string ConversationId, string Language);
    public record GetConversationResponse(Conversation conversation, List<Message> messages);
    public class GetConversation : PublicFeature<GetConversationRequest, GetConversationResponse>
    {
        public GetConversation(IRepository<Conversation> conversations, IRepository<Message> messages, IbisEngine ibisEngine)
        {
            Conversations = conversations;
            Messages = messages;
            IbisEngine = ibisEngine;
        }

        public IRepository<Conversation> Conversations { get; }
        public IRepository<Message> Messages { get; }
        public IbisEngine IbisEngine { get; }

        public async override Task<GetConversationResponse> ExecuteAsync(GetConversationRequest request)
        {
            var conversation = await Conversations.FindAsync(request.ConversationId);
            if (conversation == null)
                throw new NotFoundException($"Conversation {request.ConversationId} not found!");

            if (!conversation.Languages.Any(x => x.Name == request.Language))
            {
                conversation.AddLanguage(request.Language);
                await Conversations.UpdateAsync(conversation);
            }

            var messages = Messages.Query
                .Where(x => x.ConversationId == conversation.Id)
                .OrderBy(x => x.Timestamp)
                .ToList();

            var untranslatedMessages = messages.Where(x => !x.Translations.Any(y => y.Language == request.Language)).ToList();
            foreach (var message in untranslatedMessages)
            {
                await IbisEngine.Translate(message, request.Language);
                await Messages.UpdateAsync(message);
            }

            return new(conversation, messages);
        }
    }
}
