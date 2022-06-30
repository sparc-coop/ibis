namespace Ibis.Features.Rooms;

public record NewRoomRequest(string RoomName, List<string>? Emails);
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
        var room = new Room(request.RoomName, User.Id());

        //find current users
        foreach(string email in request.Emails!)
        {
            var user = Users.Query.Where(u => u.Email == email).FirstOrDefault();
            if(user != null)
            {
                ActiveUser newMember = new ActiveUser(user.Id, DateTime.Now, user.PrimaryLanguageId, user.ProfileImg, user.PhoneNumber);
                room.ActiveUsers.Add(newMember);
            } else
            {
                if(!room.PendingUsers.Any(x => x == email))
                    room.PendingUsers.Add(email);
            }
        }

        await Rooms.AddAsync(room);
        return new(room);
    }
}
