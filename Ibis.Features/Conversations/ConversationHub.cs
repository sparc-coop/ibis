using Ibis.Features.Conversations.Entities;
using Microsoft.AspNetCore.SignalR;
using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Conversations;

public class ConversationHub : Hub
{
    public IRepository<User> Users { get; }
    public IRepository<Conversation> Conversations { get; }

    public ConversationHub(IRepository<User> users, IRepository<Conversation> conversations)
    {
        Users = users;
        Conversations = conversations;
    }

    public async Task SendMessage(string conversationId, Message message)
    {
        await Clients.Group(conversationId).SendAsync("NewMessage", message);
    }

    public async Task AddToConversation(string conversationId, string userId, string language)
    {
        await RegisterConversationAsync(conversationId, userId);
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        //await Groups.AddToGroupAsync(Context.ConnectionId, $"{conversationId}|{language}");
    }

    public async Task RemoveFromConversation(string conversationId, string userId)
    {
        conversationId = await UnregisterConversationAsync(conversationId, userId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
        //await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{conversationId}|{language}");
    }

    private async Task RegisterConversationAsync(string conversationId, string userId)
    {
        var user = await Users.FindAsync(userId);
        await Users.ExecuteAsync(userId, user => user.JoinConversation(conversationId, Context.ConnectionId));
        await Conversations.ExecuteAsync(conversationId, conv => conv.AddUser(user!.Id, user.PrimaryLanguageId, user.PhoneNumber));
    }

    private async Task<string> UnregisterConversationAsync(string conversationId, string userId)
    {
        var user = await Users.FindAsync(userId);
        conversationId = user!.LeaveConversation(conversationId) ?? conversationId;
        await Users.UpdateAsync(user);

        await Conversations.ExecuteAsync(conversationId, conv => conv.RemoveUser(userId));

        return conversationId;
    }

    public async Task UpdateConversation(Conversation conversation)
    {
        await Conversations.UpdateAsync(conversation);
    }
}
