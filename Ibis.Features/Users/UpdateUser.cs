namespace Ibis.Features.Users;

public record UpdateUserRequest(string UserId, string FullName, string LanguageId, string Description, string pronouns);
public class UpdateUser : Feature<UpdateUserRequest, bool>
{
    public UpdateUser(IRepository<User> users)
    {
        Users = users;
    }

    public IRepository<User> Users { get; }

    public override async Task<bool> ExecuteAsync(UpdateUserRequest request)
    {
        User user = await Users.FindAsync(request.UserId);
        user.FirstName = request.FullName.Split(' ')[0];
        user.LastName = request.FullName.Split(' ')[1];
        user.PrimaryLanguageId = request.LanguageId;
        user.Description = request.Description;
        user.Pronouns = request.pronouns;

        await Users.UpdateAsync(user);
        return true;
    }
}
