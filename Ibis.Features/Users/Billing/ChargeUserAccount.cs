using Ibis._Plugins.Billing;

namespace Ibis.Users;

public record CostIncurred(Message Message, string Description, long Ticks) : Notification();
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
        var user = await Users.FindAsync(room!.HostUser.Id);
        if (room == null || user == null)
            return;

        UserCharge userCharge = new(room, notification, user);

        await Users.ExecuteAsync(room.HostUser.Id, x => x.AddCharge(notification));
        await Charges.AddAsync(userCharge);
    }
}
