using Sparc.Authentication.AzureADB2C;

namespace Ibis.Features.Users;

//public record GetUserResponse(string Id, string FullName, string Email, string Language);
public record FindUserRequest(string? Id, string? Email);
public class FindUserByEmail : Feature<FindUserRequest, User>
{
    public IRepository<User> Users { get; }
    public FindUserByEmail(IRepository<User> users)
    {
        Users = users;
    }

    public override async Task<User> ExecuteAsync(FindUserRequest req)
    {
        var user = Users.Query.Where(u => u.Email == req.Email).FirstOrDefault();

        if (user == null)
        {
            user = new(Guid.NewGuid().ToString(), req.Email!);
            await Users.AddAsync(user);
        }

        return user;
    }
}
