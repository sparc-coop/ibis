using Sparc.Authentication.AzureADB2C;

namespace Ibis.Features.Users;

public record GetUserResponse(string Id, string FullName, string? Email, string Language, string? ProfileImg, string? Pronouns, string? Description);
public class GetUser : Feature<GetUserResponse>
{
    public IRepository<User> Users { get; }
    public GetUser(IRepository<User> users)
    {
        Users = users;
    }

    public override async Task<GetUserResponse> ExecuteAsync()
    {
        var user = await Users.FindAsync(User.Id());
        if (user == null)
        {
            user = new(User.Id(), User.Email(), User.FirstName(), User.LastName());
            await Users.UpdateAsync(user);
        }

        return new(user.Id, user.FullName, user.Email, user.PrimaryLanguageId, user.ProfileImg, user.Pronouns, user.Description);
    }
}
