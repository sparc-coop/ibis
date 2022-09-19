namespace Ibis.Features.Rooms;

public record NewRoomRequest(string RoomName, List<string> Emails);
public class CreateRoom : Feature<NewRoomRequest, GetRoomResponse>
{
    public CreateRoom(IRepository<Room> rooms, IRepository<User> users)
    {
        Rooms = rooms;
        Users = users;
    }

    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }

    public async override Task<GetRoomResponse> ExecuteAsync(NewRoomRequest request)
    {
        var host = await Users.FindAsync(User.Id());
        if (host == null)
            throw new NotAuthorizedException("User not found!");

        var room = new Room(request.RoomName, host);

        //find current users
        foreach (string email in request.Emails)
        {
            var user = Users.Query.FirstOrDefault(u => u.Email == email);
            room.InviteUser(user == null ? new(email) : new(user));
        }

        await Rooms.AddAsync(room);
        return new(room);
    }
}
