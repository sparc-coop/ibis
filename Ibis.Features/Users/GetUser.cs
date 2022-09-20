using Sparc.Authentication.AzureADB2C;

namespace Ibis.Features.Users;

public class GetUser : Feature<UserAvatar>
{
    public IRepository<User> Users { get; }
    public GetUser(IRepository<User> users)
    {
        Users = users;
    }

    public override async Task<UserAvatar> ExecuteAsync()
    {
        var user = await Users.FindAsync(User.Id());
        if (user == null)
        {
            user = new(User.Email());
            await Users.UpdateAsync(user);
        }

        return user.Avatar;
    }
}
