namespace Ibis.Users;

public class UserBilling
{
    public UserBilling(string customerId, string currency)
    {
        CustomerId = customerId;
        Currency = currency;
    }

    public string CustomerId { get; set; }

    public decimal Balance { get; set; }
    public string Currency { get; set; }
}
