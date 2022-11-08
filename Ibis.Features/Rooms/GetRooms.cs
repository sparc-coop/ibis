namespace Ibis.Features.Rooms;

public class GetRooms : Feature<List<GetRoomResponse>>
{
    public GetRooms(IRepository<Room> rooms, IRepository<User> users)
    {
        Rooms = rooms;
        Users = users;
    }

    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }

    public override async Task<List<GetRoomResponse>> ExecuteAsync()
    {
        var user = await Users.GetAsync(User);
        return await ExecuteAsUserAsync(user!);
    }

    internal async Task<List<GetRoomResponse>> ExecuteAsUserAsync(User user)
    {
        var rooms = await Rooms.Query
            .Where(x => x.HostUser.Id == user.Id && x.EndDate == null)
            .OrderByDescending(x => x.LastActiveDate)
            .ToListAsync();

        return rooms.Select(x => new GetRoomResponse(x)).ToList();
    }
}
