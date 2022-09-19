namespace Ibis.Features.Rooms;

public class DeleteRoom : Feature<string, bool>
{
    public DeleteRoom(IRepository<Room> rooms)
    {
        Rooms = rooms;
    }

    public IRepository<Room> Rooms { get; }

    public async override Task<bool> ExecuteAsync(string roomId)
    {
        var room = await Rooms.FindAsync(roomId);
        if (room == null)
            throw new NotFoundException($"Room {roomId} not found!");

        room.Close();
        await Rooms.UpdateAsync(room);
        return true;
    }
}
