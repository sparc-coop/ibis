using Ibis.Features._Plugins;
using Ibis.Features.Conversations.Entities;
using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Conversations
{
    public record ConversationUser(string name, string initials, string email);
    public record ConversationUsersResponse(List<ConversationUser> users);   
    public class GetConversationUsers : Feature<string, ConversationUsersResponse>
    {
        public GetConversationUsers(IRepository<Conversation> conversations, IRepository<User> users, IbisEngine ibisEngine)
        {
            Conversations = conversations;
            Users = users;
            IbisEngine = ibisEngine;
        }

        public IRepository<Conversation> Conversations { get; }
        public IRepository<User> Users { get; }
        public IbisEngine IbisEngine { get; }

        public override async Task<ConversationUsersResponse> ExecuteAsync(string conversationId)
        {
            var convo = await Conversations.FindAsync(conversationId);
            var userList = new List<ConversationUser>();
            foreach(var item in convo.ActiveUsers)
            {
                User user = await Users.FindAsync(item.UserId);
                userList.Add(new(user.FullName, user.Initials, user.Email));
            }
            return new(userList);
        }
    }
}
