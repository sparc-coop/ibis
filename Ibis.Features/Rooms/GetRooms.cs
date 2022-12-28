using Ardalis.Specification;
using System.Security.Claims;

namespace Ibis.Features.Rooms;

public class GetRooms : Specification<Room>
{
    public GetRooms(ClaimsPrincipal user)
    {
        Query
            .Where(x => x.HostUserId == user.Id() && x.EndDate == null)
            .OrderByDescending(x => x.LastActiveDate);
    }
}