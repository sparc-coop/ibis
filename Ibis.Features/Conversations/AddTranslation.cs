using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Conversations
{
    public record AddTranslationRequest(List<Message> Messages, string ConversationId, string Language);
    //public record AddTranslationResponse(IEnumerable<string> Files, string ConversationId, string Language);
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
            var untranslatedMessages = request.Messages.Where(x => !x.HasTranslation(request.Language)).ToList();
            var updatedMessages = new List<Message>();

            foreach (var message in untranslatedMessages)
            {
                await IbisEngine.TranslateAsync(message, request.Language);
                await Messages.UpdateAsync(message);
            }
            return updatedMessages;
        }
    }
}
