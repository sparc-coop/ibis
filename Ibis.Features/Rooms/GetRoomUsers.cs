namespace Ibis.Features.Rooms;

public record RoomUser(string Name, string Initials, string Email);
public record RoomUsersResponse(List<RoomUser> Users);   
public class GetRoomUsers : Feature<string, RoomUsersResponse>
{
    public GetRoomUsers(IRepository<Room> rooms, IRepository<User> users)
    {
        Rooms = rooms;
        Users = users;
    }

    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }

    public override async Task<RoomUsersResponse> ExecuteAsync(string roomId)
    {
        var room = await Rooms.FindAsync(roomId);
        if (room == null)
            throw new NotFoundException("Room not found!");


        var userList = new List<RoomUser>();
        foreach(var item in room.ActiveUsers)
        {
            var user = await Users.FindAsync(item.UserId);
            if (user != null)
                userList.Add(new(user.FullName ?? "", user.Initials, user.Email ?? ""));
        }
        if(room.PendingUsers != null)
		{
            foreach (var pendingUser in room.PendingUsers)
            {
                userList.Add(new("", "", pendingUser));
            }
        }

        return new(userList);
    }
}
