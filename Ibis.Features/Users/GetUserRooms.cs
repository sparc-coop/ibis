namespace Ibis.Features.Users;

public class GetUserRooms : Feature<string, List<GetRoomResponse>>
{
    public GetUserRooms(IRepository<Room> rooms)
    {
        Rooms = rooms;
    }

    public IRepository<Room> Rooms { get; }
 
    public override async Task<List<GetRoomResponse>> ExecuteAsync(string userId)
    {
        var rooms = await Rooms.Query.Where(x => x.HostUserId == userId && x.EndDate == null).ToListAsync(); // || x.ActiveUsers.Any(y => y.UserId == userId)
        return rooms.Select(x => new GetRoomResponse(x)).ToList();
    }
}
