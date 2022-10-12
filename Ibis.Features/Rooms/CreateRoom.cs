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

    public override async Task<GetRoomResponse> ExecuteAsync(NewRoomRequest request)
    {
        var host = await Users.GetAsync(User);
        if (host == null)
            throw new NotAuthorizedException("User not found!");

        return await ExecuteAsUserAsync(request, host);
    }

    internal async Task<GetRoomResponse> ExecuteAsUserAsync(NewRoomRequest request, User host)
    {
        var room = new Room(request.RoomName, host);

        var existingRoom = Rooms.Query.FirstOrDefault(x => x.HostUser.Id == host.Id && x.Slug == room.Slug);
        if (existingRoom != null)
            throw new ForbiddenException($"A room already exists in your account with the name '{room.Slug}'. Please choose a different name.");

        //find current users
        foreach (string email in request.Emails)
        {
            var user = Users.Query.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                room.InviteUser(new UserAvatar(email, email));
            }
            else
            {
                room.InviteUser(user.Avatar);
            }
        }

        await Rooms.AddAsync(room);
        return new(room);
    }
}
