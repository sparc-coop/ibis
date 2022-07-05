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
        var userList = new List<RoomUser>();
        foreach(var item in room.ActiveUsers)
        {
            User user = await Users.FindAsync(item.UserId);
            userList.Add(new(user.FullName, user.Initials, user.Email));
        }
        if(room.PendingUsers != null)
		{
            foreach (var pendingUser in room.PendingUsers)
            {
                userList.Add(new(null, null, pendingUser));
            }
        }

        return new(userList);
    }
}
