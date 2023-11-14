namespace Ibis;


public record AddUrlToRoomRequest(string RoomId, string Url);
public class AddUrlToRoom : Feature<AddUrlToRoomRequest, bool>
{
    public AddUrlToRoom(IRepository<Room> rooms)
    {
        Rooms = rooms;
    }
    public IRepository<Room> Rooms { get; }

    public override async Task<bool> ExecuteAsync(AddUrlToRoomRequest request)
    {
        var room = await Rooms.FindAsync(request.RoomId);
        if (room == null)
            throw new NotFoundException("Room not found");

        room.SetUrl(request.Url);        
        await Rooms.UpdateAsync(room);

        return true;
    }
}