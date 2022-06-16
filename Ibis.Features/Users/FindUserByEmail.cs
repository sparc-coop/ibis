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

        //var user = await Users.FindAsync(User.Id());
        if (user == null)
        {
            user = new()
            {
                Id = Guid.NewGuid().ToString(),
            //FirstName = User.FirstName(),
            //LastName = User.LastName(),
            Email = req.Email
            };
            await Users.AddAsync(user);
            //await Users.UpdateAsync(user);
        }

        return user;
    }
}
