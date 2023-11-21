using Microsoft.EntityFrameworkCore;

namespace Ibis.Rooms;

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
    public string Url { get; set; }

    public GetRoomResponse(Room room, List<UserAvatar>? users = null)
    {
        RoomId = room.Id;
        RoomType = room.RoomType;
        LastActiveDate = room.LastActiveDate;
        StartDate = room.StartDate;
        Name = room.Name;
        Slug = room.Slug;
        Users = room.Users;
        Url = room.Url;

        ReplaceUsersWithCurrent(users);

        HostUserId = room.HostUser.Id;
    }

    private void ReplaceUsersWithCurrent(List<UserAvatar>? users)
    {
        if (users != null)
        {
            foreach (var user in users)
            {
                var idx = Users!.FindIndex(u => u.Id == user.Id);
                if (idx >= 0)
                    Users[idx] = user;
            }
        }
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

        var userIds = room.Users.Select(u => u.Id).ToList();
        var users = await Users.Query
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(x => x.Avatar)
            .ToListAsync();

        return new(room, users);
    }
}
