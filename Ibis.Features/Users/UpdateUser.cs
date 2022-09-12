namespace Ibis.Features.Users;

public record UpdateUserRequest(string FullName, string LanguageId, string Pronouns, string Description);
public class UpdateUser : Feature<UpdateUserRequest, bool>
{
    public UpdateUser(IRepository<User> users)
    {
        Users = users;
    }

    public IRepository<User> Users { get; }

    public override async Task<bool> ExecuteAsync(UpdateUserRequest request)
    {
        var user = await Users.FindAsync(User.Id());
        if (user == null)
            throw new NotFoundException("User not found!");

        user.UpdateProfile(request.FullName, request.LanguageId, request.Pronouns, request.Description);

        
        await Users.UpdateAsync(user);
       
        return true;
    }
}
