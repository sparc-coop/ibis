using Sparc.Authentication.AzureADB2C;
using Sparc.Core;
using Sparc.Features;
using System.Security.Claims;

namespace Ibis.Features.Users
{
    public record UpdateUserRequest(string userId, string fullName, string languageId);
    public class UpdateUser : Feature<UpdateUserRequest, bool>
    {
        public UpdateUser(IRepository<User> users)
        {
            Users = users;
        }

        public IRepository<User> Users { get; }

        public override async Task<bool> ExecuteAsync(UpdateUserRequest request)
        {
            User user = await Users.FindAsync(request.userId);
            user.FirstName = request.fullName.Split(' ')[0];
            user.LastName = request.fullName.Split(' ')[1];
            user.PrimaryLanguageId = request.languageId;
            await Users.UpdateAsync(user);
            return true;
        }
    }
}
