using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Ibis.Features.Rooms;

public class GetRooms : Feature<string, List<GetRoomResponse>>
{
    public GetRooms(IRepository<Room> rooms, IRepository<User> users)
    {
        Rooms = rooms;
        Users = users;
    }

    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }

    public override async Task<List<GetRoomResponse>> ExecuteAsync(string roomType)
    {
        var user = await Users.GetAsync(User);
        return await ExecuteAsUserAsync(user!, roomType);
    }

    internal async Task<List<GetRoomResponse>> ExecuteAsUserAsync(User user, string? roomType = null)
    {
        var rooms = await Rooms.Query
            .Where(x => x.HostUser.Id == user.Id && (roomType != null ? x.RoomType == roomType : x.RoomType.Any()) && x.EndDate == null)
            .OrderByDescending(x => x.LastActiveDate)
            .ToListAsync();

        return rooms.Select(x => new GetRoomResponse(x)).ToList();
    }
}
