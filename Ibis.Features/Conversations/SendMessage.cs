using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Microsoft.AspNetCore.SignalR;
using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Conversations
{
    public record SendMessageRequest(string ConversationId, string Message, string Language);
    public class SendMessage : PublicFeature<SendMessageRequest, Message>
    {
        public SendMessage(IRepository<Message> messages, IRepository<Conversation> conversations, IHubContext<ConversationHub> conversation, IbisEngine ibisEngine)
        {
            Messages = messages;
            Conversations = conversations;
            Conversation = conversation;
            IbisEngine = ibisEngine;
        }

        public IRepository<Message> Messages { get; }
        public IRepository<Conversation> Conversations { get; }
        public IHubContext<ConversationHub> Conversation { get; }
        public IbisEngine IbisEngine { get; }

        public override async Task<Message> ExecuteAsync(SendMessageRequest request)
        {
            var message = new Message(request.ConversationId, "userId", request.Language ?? User.Language(), SourceTypes.Text);
            message.SetText(request.Message);

            // Translate message to all other languages
            var conversation = await Conversations.FindAsync(request.ConversationId);
            await IbisEngine.TranslateAsync(message, conversation!.Languages);
            await IbisEngine.SpeakAsync(message);

            await Messages.AddAsync(message);

            await Conversation.Clients.Group($"{request.ConversationId}").SendAsync("NewMessage", message);
            return message;
        }
    }
}
