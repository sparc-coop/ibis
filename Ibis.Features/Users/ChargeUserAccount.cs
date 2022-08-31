using Ibis.Features.Sparc.Realtime;

namespace Ibis.Features.Users;

public record CostIncurred(Message Message, string Description, decimal Amount) : GroupNotification(Message.RoomId);
public class ChargeUserAccount : BackgroundFeature<CostIncurred>
{
    public ChargeUserAccount(IRepository<UserCharge> charges, IRepository<User> users, IRepository<Room> rooms)
    {
        Charges = charges;
        Users = users;
        Rooms = rooms;
    }

    public IRepository<UserCharge> Charges { get; }
    public IRepository<User> Users { get; }
    public IRepository<Room> Rooms { get; }

    public override async Task ExecuteAsync(CostIncurred notification)
    {
        var room = await Rooms.FindAsync(notification.Message.RoomId);
        if (room == null)
            return;
        
        UserCharge userCharge = new(room.HostUser.Id, room.Id, notification.Message.Id, notification.Description, notification.Amount * 1.2M);

        await Users.ExecuteAsync(room.HostUser.Id, x => x.AddCharge(userCharge));
        await Charges.AddAsync(userCharge);
    }
}
