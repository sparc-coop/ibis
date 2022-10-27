﻿using Stripe;

namespace Ibis.Features.Users;

public class UserCharge : Root<string>
{
    public string UserId { get; set; }
    public string? RoomId { get; set; }
    public string? MessageId { get; set; }
    public string Description { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentIntent { get; set; }
    
    public UserCharge(string userId, PaymentIntent paymentIntent)
    {
        Id = paymentIntent.Id;
        UserId = userId;
        Description = "Funds Added";
        Timestamp = DateTime.UtcNow;
        Amount = paymentIntent.Amount / 100M;
        PaymentIntent = paymentIntent.ToJson();
    }

    public UserCharge(string userId, string roomId, string description, decimal amount)
    {
        Id = Guid.NewGuid().ToString();
        UserId = userId;
        RoomId = roomId;
        Description = description;
        Amount = amount;
        Timestamp = DateTime.UtcNow;
    }

    public UserCharge(string userId, string roomId, string? messageId, string description, decimal amount)
        : this(userId, roomId, description, amount)
    {
        MessageId = messageId;
    }
}
