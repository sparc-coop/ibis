namespace Ibis.Features.Rooms;

public record NewRoomRequest(string RoomName, List<RoomUser>? Participants);
public class CreateRoom : Feature<NewRoomRequest, GetRoomResponse>
{
    public CreateRoom(IRepository<Room> rooms)
    {
        Rooms = rooms;
    }

    public IRepository<Room> Rooms { get; }

    public async override Task<GetRoomResponse> ExecuteAsync(NewRoomRequest request)
    {
        var room = new Room(request.RoomName, User.Id());
        await Rooms.AddAsync(room);
        return new(room);
    }
}
