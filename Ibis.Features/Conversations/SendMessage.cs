using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Microsoft.AspNetCore.SignalR;
using Sparc.Core;
using Sparc.Features;
using Sparc.Notifications.Twilio;

namespace Ibis.Features.Conversations
{
    public record SendMessageRequest(string ConversationId, string? Message, string Language, string? MessageId, string? ModifiedMessage);
    public class SendMessage : Feature<SendMessageRequest, Message>
    {
        public SendMessage(IRepository<Message> messages,
            IRepository<Conversation> conversations,
            IRepository<User> users,
            IHubContext<ConversationHub> conversation,
            IbisEngine ibisEngine,
            TwilioService twilio)
        {
            Messages = messages;
            Conversations = conversations;
            Users = users;
            Conversation = conversation;
            IbisEngine = ibisEngine;
            Twilio = twilio;
        }

        public IRepository<Message> Messages { get; }
        public IRepository<Conversation> Conversations { get; }
        public IRepository<User> Users { get; }
        public IHubContext<ConversationHub> Conversation { get; }
        public IbisEngine IbisEngine { get; }
        public TwilioService Twilio { get; }

        public override async Task<Message> ExecuteAsync(SendMessageRequest request)
        {
            var user = await Users.FindAsync(User.Id());
            Message message;
            Conversation conversation;

            // Message from text input
            if (request.MessageId == null)
            {
                message = new Message(request.ConversationId, User.Id(), request.Language ?? user!.PrimaryLanguageId, SourceTypes.Text, user.FullName, user.Initials);
                message.UserName = user.FullName;
                message.SetText(request.Message);

                // Translate and Speak
                conversation = await Conversations.FindAsync(request.ConversationId);
                conversation.LastActiveDate = DateTime.UtcNow;
                await IbisEngine.TranslateAsync(message, conversation!.Languages);
                await IbisEngine.SpeakAsync(message);
                await Messages.AddAsync(message);
            } 
            // Message from Upload
            else if (request.MessageId != null && request.ModifiedMessage == null)
            {
                message = await Messages.FindAsync(request.MessageId);
                message.SetModifiedText(request.ModifiedMessage);

                // Translate, Speak is Audio from Upload
                conversation = await Conversations.FindAsync(request.ConversationId);
                conversation.LastActiveDate = DateTime.UtcNow;
                await IbisEngine.TranslateAsync(message, conversation!.Languages);
                await Messages.UpdateAsync(message);
            } 
            // Message from Upload AND modified
            else
            {
                message = await Messages.FindAsync(request.MessageId);
                message.SetModifiedText(request.ModifiedMessage);

                // Translate modified message and speak
                conversation = await Conversations.FindAsync(request.ConversationId);
                conversation.LastActiveDate = DateTime.UtcNow;
                await IbisEngine.TranslateAsync(message, conversation!.Languages);
                await IbisEngine.SpeakAsync(message);
                await Messages.UpdateAsync(message);
            }

            await Conversations.UpdateAsync(conversation);
            await Conversation.Clients.Group($"{request.ConversationId}").SendAsync("NewMessage", message);

            var usersToSms = conversation.ActiveUsers.Where(x => x.PhoneNumber != null && message.HasTranslation(x.Language)).ToList();
            foreach (var userToSms in usersToSms)
                await Twilio.SendSmsAsync(userToSms.PhoneNumber!, message.GetTranslation(userToSms.Language));

            return message;
        }
    }
}
