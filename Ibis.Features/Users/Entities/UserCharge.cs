using Stripe;

namespace Ibis.Users;

public class UserCharge : Entity<string>
{
    public string UserId { get; set; }
    public string? RoomId { get; set; }
    public string? MessageId { get; set; }
    public string Description { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal Amount { get; set; }
    public long Ticks { get; set; }
    public string Currency { get; set; }
    public string? PaymentIntent { get; set; }

    public UserCharge()
    {
        UserId = "";
        Currency = "";
        Description = "";
    }
    
    public UserCharge(string userId, PaymentIntent paymentIntent)
    {
        Id = paymentIntent.Id;
        UserId = userId;
        Description = "Funds Added";
        Timestamp = DateTime.UtcNow;
        Currency = paymentIntent.Currency.ToUpper();
        Amount = paymentIntent.LocalAmount();
        PaymentIntent = paymentIntent.ToJson();

        Ticks = paymentIntent.Metadata.TryGetValue("Ticks", out var ticksStr) && long.TryParse(ticksStr, out var ticksVal)
            ? ticksVal
            : 0;
    }

    public UserCharge(Room room, CostIncurred cost, User user)
    {
        Id = Guid.NewGuid().ToString();
        UserId = user.Id;
        RoomId = room.Id;
        MessageId = cost.Message?.Id;
        Description = cost.Description;
        Amount = 0;
        Ticks = cost.Ticks;
        Timestamp = DateTime.UtcNow;
        Currency = "Ticks";
    }

    internal static UserCharge Fail(Event stripeEvent)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        var customer = new CustomerService().Get(paymentIntent!.CustomerId);
        var userId = customer.Metadata["IbisUserId"];
        return new(userId, paymentIntent!);
    }

    internal static UserCharge Process(Event stripeEvent, User user)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        _ = new CustomerService().Get(paymentIntent!.CustomerId);
        var userId = paymentIntent.Metadata["IbisUserId"];
        if (userId != user.Id)
            throw new Exception("Billing issue: User IDs did not match");

        UserCharge userCharge = new(user.Id, paymentIntent!);
        user.Refill(userCharge.Ticks);
        return userCharge;
    }
}
