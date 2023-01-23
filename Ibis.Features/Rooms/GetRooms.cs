using Microsoft.EntityFrameworkCore;

namespace Ibis.Rooms;

public record GetRoomsResponse(List<GetRoomResponse> HostedRooms, List<GetRoomResponse> InvitedRooms);
public class GetRooms : Feature<GetRoomsResponse>
{
    public GetRooms(IRepository<Room> rooms, IRepository<User> users)
    {
        Rooms = rooms;
        Users = users;
    }

    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }

    public override async Task<GetRoomsResponse> ExecuteAsync()
    {
        var user = await Users.GetAsync(User);
        return await ExecuteAsUserAsync(user!);
    }

    internal async Task<GetRoomsResponse> ExecuteAsUserAsync(User user)
    {
        var hostedRooms = await Rooms.Query
            .Where(x => x.HostUser.Id == user.Id && x.EndDate == null)
            .OrderByDescending(x => x.LastActiveDate)
            .ToListAsync();

        var invitedRooms = Rooms.FromSqlRaw("SELECT VALUE r FROM r JOIN u IN r.Users WHERE u.Id = {0}", user.Id)
            .Where(x => x.EndDate == null)
            .OrderByDescending(x => x.LastActiveDate)
            .ToList()
            .Where(x => !hostedRooms.Any(y => y.RoomId == x.RoomId))
            .ToList();

        var userIds = hostedRooms.SelectMany(x => x.Users).Select(x => x.Id)
            .Union(invitedRooms.SelectMany(x => x.Users).Select(x => x.Id))
            .ToList();
        
        var users = await Users.Query
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(x => x.Avatar)
            .ToListAsync();

        return new(
            hostedRooms.Select(x => new GetRoomResponse(x, users)).ToList(),
            invitedRooms.Select(x => new GetRoomResponse(x, users)).ToList());
    }
}
