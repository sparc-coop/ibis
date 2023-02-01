namespace Ibis.Users;

public class GetUser : Feature<UserAvatar>
{
    public IRepository<User> Users { get; }
    public GetUser(IRepository<User> users)
    {
        Users = users;
    }

    public override async Task<UserAvatar> ExecuteAsync()
    {
        var user = await Users.GetAsync(User);
        if (user == null)
        {
            user = new(User.Id(), User.Email()!);
            await Users.UpdateAsync(user);
        }

        return user.Avatar;
    }
}
