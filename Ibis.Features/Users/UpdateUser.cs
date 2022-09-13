namespace Ibis.Features.Users;

public class UpdateUser : Feature<UserSummary, bool>
{
    public UpdateUser(IRepository<User> users)
    {
        Users = users;
    }

    public IRepository<User> Users { get; }

    public override async Task<bool> ExecuteAsync(UserSummary request)
    {
        var user = await Users.FindAsync(User.Id());
        if (user == null)
            throw new NotFoundException("User not found!");

        user.UpdateProfile(request.Name, request.Language, request.Pronouns, request.Description);

        
        await Users.UpdateAsync(user);
       
        return true;
    }
}
