using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Conversations
{
    public record AddTranslationRequest(string ConversationId, string Language);
    public class AddTranslation : Feature<AddTranslationRequest, List<Message>>
    {
        public AddTranslation(IRepository<Conversation> conversations, IRepository<Message> messages, IbisEngine ibisEngine)
        {
            Conversations = conversations;
            Messages = messages;
            IbisEngine = ibisEngine;
        }

        public IRepository<Conversation> Conversations { get; }
        public IRepository<Message> Messages { get; }
        public IbisEngine IbisEngine { get; }

        public override async Task<List<Message>> ExecuteAsync(AddTranslationRequest request)
        {
            var untranslatedMessages = await Messages.Query.Where(x => x.ConversationId == request.ConversationId).ToListAsync();

            foreach (var message in untranslatedMessages)
            {
                await IbisEngine.TranslateAsync(message, request.Language);
            }

            return Messages.Query.Where(x => x.ConversationId == request.ConversationId).ToList();
        }
    }
}
