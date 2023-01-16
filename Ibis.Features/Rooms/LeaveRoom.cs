namespace Ibis.Rooms;

public record LeaveRoomRequest(string RoomId);
public class LeaveRoom : Feature<LeaveRoomRequest, bool>
{
    public LeaveRoom(IRepository<Room> rooms, IRepository<User> users, IRepository<Message> messages)
    {
        Rooms = rooms;
        Users = users;
        Messages = messages;
    }

    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }
    public IRepository<Message> Messages { get; }

    public async override Task<bool> ExecuteAsync(LeaveRoomRequest request)
    {
        var room = await Rooms.FindAsync(request.RoomId);
        var user = await Users.GetAsync(User);
        if (room == null || user == null)
            throw new NotFoundException($"Room {request.RoomId} not found!");

        await Users.ExecuteAsync(User.Id(), user => user.LeaveRoom(request.RoomId));
        await Rooms.ExecuteAsync(request.RoomId, room => room.RemoveActiveUser(user));
        
        return true;
    }
}
