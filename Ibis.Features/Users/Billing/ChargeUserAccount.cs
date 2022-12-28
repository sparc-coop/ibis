using Sparc.Blossom;

namespace Ibis.Features.Users;

public record CostIncurred(Message Message, string Description, decimal Amount) : Notification();
public class ChargeUserAccount : RealtimeFeature<CostIncurred>
{
    public ChargeUserAccount(IRepository<UserCharge> charges, IRepository<User> users, IRepository<Room> rooms, ExchangeRates exchangeRates)
    {
        Charges = charges;
        Users = users;
        Rooms = rooms;
        ExchangeRates = exchangeRates;
    }

    public IRepository<UserCharge> Charges { get; }
    public IRepository<User> Users { get; }
    public IRepository<Room> Rooms { get; }
    public ExchangeRates ExchangeRates { get; }

    public override async Task ExecuteAsync(CostIncurred notification)
    {
        var room = await Rooms.FindAsync(notification.Message.RoomId);
        var user = await Users.FindAsync(room!.HostUserId);
        if (room == null || user == null)
            return;

        var amountInUsersCurrency = await ExchangeRates.ConvertAsync(notification.Amount, "USD", user.BillingInfo!.Currency);
        UserCharge userCharge = new(room, notification, user, amountInUsersCurrency);

        await Users.ExecuteAsync(room.HostUserId, x => x.AddCharge(userCharge));
        await Charges.AddAsync(userCharge);
    }
}
