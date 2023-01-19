namespace Ibis.Features.Rooms;

public record JoinRoomRequest(string RoomId);
public record GetRoomResponse
{
    public string RoomId { get; set; }
    public string RoomType { get; set; }
    public string Slug { get; set; }
    public DateTime? LastActiveDate { get; set; }
    public DateTime StartDate { get; private set; }
    public string Name { get; private set; }
    public List<UserAvatar>? Users { get; set; }
    public string HostUserId { get; set; }

    public GetRoomResponse(Room room)
    {
        RoomId = room.Id;
        RoomType = room.RoomType;
        LastActiveDate = room.LastActiveDate;
        StartDate = room.StartDate;
        Name = room.Name;
        Slug = room.Slug;
        Users = room.Users;
        HostUserId = room.HostUser.Id;
    }
}

public class JoinRoom : Feature<JoinRoomRequest, GetRoomResponse>
{
    public JoinRoom(IRepository<Room> rooms, IRepository<User> users, IRepository<Message> messages)
    {
        Rooms = rooms;
        Users = users;
        Messages = messages;
    }

    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }
    public IRepository<Message> Messages { get; }

    public async override Task<GetRoomResponse> ExecuteAsync(JoinRoomRequest request)
    {
        var room = await Rooms.FindAsync(request.RoomId);
        var user = await Users.GetAsync(User);
        if (room == null || user == null)
            throw new NotFoundException($"Room {request.RoomId} not found!");

        await Users.ExecuteAsync(User.Id(), user => user.JoinRoom(room.Id));
        await Rooms.ExecuteAsync(room.Id, room => room.AddActiveUser(user));
        
        return new(room);
    }
}
