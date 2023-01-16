namespace Ibis.Users;

public class GetUserBalance : Feature<long>
{
    public IRepository<User> Users { get; }
    public GetUserBalance(IRepository<User> users)
    {
        Users = users;
    }

    public override async Task<long> ExecuteAsync()
    {
        var user = await Users.GetAsync(User);
        return user?.BillingInfo.TicksBalance ?? 0;
    }
}
