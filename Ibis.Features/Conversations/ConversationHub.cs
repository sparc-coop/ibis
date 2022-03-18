using Ibis.Features.Conversations.Entities;
using Microsoft.AspNetCore.SignalR;

namespace Ibis.Features.Conversations;

public class ConversationHub : Hub
{
    public async Task SendMessage(string conversationId, Message message)
    {
        await Clients.Group(conversationId).SendAsync("NewMessage", message);
    }

    public async Task AddToConversation(string conversationId, string language)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        //await Groups.AddToGroupAsync(Context.ConnectionId, $"{conversationId}|{language}");
    }
}
