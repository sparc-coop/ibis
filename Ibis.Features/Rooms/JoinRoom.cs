using Microsoft.AspNetCore.SignalR;

namespace Ibis.Features.Rooms;

public record JoinRoomRequest(string RoomId);
public record GetRoomResponse
{
    public string RoomId { get; set; }
    public DateTime? LastActiveDate { get; set; }
    public DateTime StartDate { get; private set; }
    public string Name { get; private set; }
    public List<UserAvatar>? Users { get; set; }
    public List<Message>? Messages { get; set; }

    public GetRoomResponse(Room room, List<Message>? messages = null)
    {
        RoomId = room.Id;
        LastActiveDate = room.LastActiveDate;
        StartDate = room.StartDate;
        Name = room.Name;
        Users = room.Users;
        Messages = messages;
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
        var user = await Users.FindAsync(User.Id());
        if (room == null || user == null)
            throw new NotFoundException($"Room {request.RoomId} not found!");

        await Users.ExecuteAsync(User.Id(), user => user.JoinRoom(request.RoomId));
        await Rooms.ExecuteAsync(request.RoomId, room => room.AddActiveUser(user));

        var messages = await Messages
                .Query
                .Where(message => message.RoomId == request.RoomId && message.Language == user.Avatar.Language)
                .OrderBy(x => x.Timestamp)
                .ToListAsync();

        return new(room, messages);
    }
}
