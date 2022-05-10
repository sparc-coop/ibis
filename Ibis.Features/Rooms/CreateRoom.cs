namespace Ibis.Features.Rooms;

public class CreateRoom : Feature<GetRoomResponse>
{
    public CreateRoom(IRepository<Room> rooms)
    {
        Rooms = rooms;
    }

    public IRepository<Room> Rooms { get; }

    public async override Task<GetRoomResponse> ExecuteAsync()
    {
        var room = new Room("Test Room", User.Id());
        await Rooms.AddAsync(room);
        return new(room);
    }
}
