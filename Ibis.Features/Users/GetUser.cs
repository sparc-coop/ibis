using Sparc.Authentication.AzureADB2C;

namespace Ibis.Features.Users;

public class GetUser : Feature<UserSummary>
{
    public IRepository<User> Users { get; }
    public GetUser(IRepository<User> users)
    {
        Users = users;
    }

    public override async Task<UserSummary> ExecuteAsync()
    {
        var user = await Users.FindAsync(User.Id());
        if (user == null)
        {
            user = new(User.Id(), User.Email(), User.FirstName(), User.LastName());
            await Users.UpdateAsync(user);
        }

        return new(user);
    }
}
