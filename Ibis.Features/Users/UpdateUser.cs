namespace Ibis.Features.Users;

public class UpdateUser : Feature<UserAvatar, bool>
{
    public UpdateUser(IRepository<User> users)
    {
        Users = users;
    }

    public IRepository<User> Users { get; }

    public override async Task<bool> ExecuteAsync(UserAvatar avatar)
    {
        var user = await Users.FindAsync(User.Id());
        if (user == null)
            throw new NotFoundException("User not found!");

        user.UpdateAvatar(avatar);

        
        await Users.UpdateAsync(user);
       
        return true;
    }
}
