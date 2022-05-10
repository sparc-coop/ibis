namespace Ibis.Features.Users;

public class GetUserRooms : Feature<string, List<Room>>
{
    public GetUserRooms(IRepository<Room> rooms)
    {
        Rooms = rooms;
    }

    public IRepository<Room> Rooms { get; }
 
    public override async Task<List<Room>> ExecuteAsync(string userId)
    {
        return await Rooms.Query.Where(x => x.HostUserId == userId).ToListAsync(); // || x.ActiveUsers.Any(y => y.UserId == userId)
    }
}
