using Ibis.Features._Plugins;
using Sparc.Core;
using Sparc.Features;

namespace Ibis.Features.Users
{
    public record InviteUserRequest(string conversationId, string email);
    public class InviteUser : Feature<InviteUserRequest, bool>
    {
        public override async Task<bool> ExecuteAsync(InviteUserRequest request)
        {

            return true;
        }
    }
}
