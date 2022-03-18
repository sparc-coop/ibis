using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Microsoft.AspNetCore.SignalR;
using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Conversations
{
    public record SendMessageRequest(string ConversationId, string Message);
    public class SendMessage : PublicFeature<SendMessageRequest, Message>
    {
        public SendMessage(IRepository<Message> messages, IHubContext conversation)
        {
            Messages = messages;
            Conversation = conversation;
        }

        public IRepository<Message> Messages { get; }
        public IHubContext Conversation { get; }

        public override async Task<Message> ExecuteAsync(SendMessageRequest request)
        {
            var message = new Message(request.ConversationId, "userId", User.Language(), SourceTypes.Text);
            await Messages.AddAsync(message);
            await Conversation.Clients.Group($"{request.ConversationId}|{User.Language()}").SendAsync("NewMessage", message);
            return message;
        }
    }
}
