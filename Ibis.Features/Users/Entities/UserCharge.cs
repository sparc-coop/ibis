using Stripe;

namespace Ibis.Features.Users;

public class UserCharge : Root<string>
{
    public string UserId { get; set; }
    public string? RoomId { get; set; }
    public string? MessageId { get; set; }
    public string Description { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal Amount { get; set; }
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
    }

    public UserCharge(Room room, CostIncurred cost, User user, decimal amountInUsersCurrency)
    {
        Id = Guid.NewGuid().ToString();
        UserId = user.Id;
        RoomId = room.Id;
        MessageId = cost.Message?.Id;
        Description = cost.Description;
        Amount = amountInUsersCurrency;
        Timestamp = DateTime.UtcNow;
        Currency = user.BillingInfo!.Currency;
    }
}
