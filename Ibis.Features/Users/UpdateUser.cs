using Sparc.Authentication.AzureADB2C;
using Sparc.Core;
using Sparc.Features;
using System.Security.Claims;

namespace Ibis.Features.Users
{
    public class UpdateUser : Feature<GetUserResponse>
    {
        public UpdateUser(IRepository<User> users)
        {
            Users = users;
        }

        public IRepository<User> Users { get; }

        public override async Task<GetUserResponse> ExecuteAsync()
        {
            var user = await Users.FindAsync(User.Id());
            if (user == null)
            {
                user = new()
                {
                    Id = User.Id(),
                    FirstName = User.FirstName(),
                    LastName = User.LastName(),
                    Email = User.Email()
                };
                await Users.UpdateAsync(user);
            }

            return new(user.Id, user.FullName, user.Email, user.PrimaryLanguageId);
        }
    }
}
