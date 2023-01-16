﻿namespace Ibis.Users;

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
        return user?.BillingInfo?.Balance ?? 0M;
    }
}
