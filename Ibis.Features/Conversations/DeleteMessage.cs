using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Microsoft.AspNetCore.SignalR;
using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Conversations
{
    public record DeleteMessageRequest(string ConversationId, string MessageId);
    public class DeleteMessage : Feature<DeleteMessageRequest, Conversation>
    {
        public IRepository<Conversation> Conversations { get; }
        public IRepository<Message> Messages { get; }

        public DeleteMessage(IRepository<Conversation> conversations, IRepository<Message> messages)
        {
            Conversations = conversations;
            Messages = messages;
        }

        public override async Task<Conversation> ExecuteAsync(DeleteMessageRequest request)
        {
            var message = await Messages.FindAsync(request.MessageId);
            await Messages.DeleteAsync(message);
            return await Conversations.FindAsync(request.ConversationId);
        }
    }
}
