using Ibis.Features.Conversations.Entities;
using Microsoft.AspNetCore.Mvc;
using Sparc.Core;
using Twilio.AspNet.Common;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace Ibis.Features.Conversations;

public class ReceiveSms : TwilioController
{
    public ReceiveSms(IRepository<User> users, IRepository<Conversation> conversations, SendMessage sendMessage)
    {
        Users = users;
        Conversations = conversations;
        SendMessage = sendMessage;
    }

    public IRepository<User> Users { get; }
    public IRepository<Conversation> Conversations { get; }
    public SendMessage SendMessage { get; }

    public async Task<TwiMLResult> Index(SmsRequest incomingMessage)
    {
        // Find active conversation ID & user
        var user = Users.Query
            .Where(x => x.PhoneNumber == incomingMessage.From)
            .FirstOrDefault();

        var conversationId = user?.ActiveConversations
            .OrderByDescending(x => x.JoinDate)
            .FirstOrDefault()?.ConversationId;        

        if (user != null && conversationId != null)
        {
            var request = new SendMessageRequest(conversationId, incomingMessage.Body, user.PrimaryLanguageId);
            await SendMessage.ExecuteAsync(request);
        }

        MessagingResponse response = new();
        return TwiML(response);
    }
}
