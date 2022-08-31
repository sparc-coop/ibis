namespace Ibis.Features.Rooms;

public class GetRooms : Feature<string, List<GetRoomResponse>>
{
    public GetRooms(IRepository<Room> rooms)
    {
        Rooms = rooms;
    }

    public IRepository<Room> Rooms { get; }

    public override async Task<List<GetRoomResponse>> ExecuteAsync(string userId)
    {
        var rooms = await Rooms.Query
            .Where(x => x.HostUser.Id == userId && x.EndDate == null)
            .ToListAsync();

        return rooms.Select(x => new GetRoomResponse(x)).ToList();
    }
}
