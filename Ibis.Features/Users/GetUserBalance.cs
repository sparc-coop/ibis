namespace Ibis.Features.Users;

public class GetUserBalance : Feature<decimal>
{
    public IRepository<User> Users { get; }
    public GetUserBalance(IRepository<User> users)
    {
        Users = users;
    }

    public override async Task<decimal> ExecuteAsync()
    {
        var user = await Users.GetAsync(User);
        return user?.Balance ?? 0M;
    }
}
