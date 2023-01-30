namespace Ibis.Users;

public class UserBilling
{
    public UserBilling()
    {
        Currency = "USD";
    }
    
    public UserBilling(string customerId, long ticksBalance, string currency)
    {
        CustomerId = customerId;
        TicksBalance = ticksBalance;
        Currency = currency;
    }

    public string? CustomerId { get; set; }

    public long TicksBalance { get; set; }
    public string Currency { get; set; }
}
