using Ardalis.Specification;

namespace Ibis.Rooms.Queries;

public class MyRooms : Specification<Room>
{
    public MyRooms(User user, string roomType)
    {
        Query
            .Where(x => x.HostUser.Id == user.Id 
                && (roomType != null ? x.RoomType == roomType : x.RoomType.Any()) 
                && x.EndDate == null)
            .OrderByDescending(x => x.LastActiveDate);

        // Add invited rooms
        //Query.FromSqlRaw("SELECT VALUE r FROM r JOIN u IN r.Users WHERE u.Id = {0}", user.Id)
        //    .Where(x => (roomType != null ? x.RoomType == roomType : x.RoomType.Any()) && x.EndDate == null)
        //    .OrderByDescending(x => x.LastActiveDate)
        //    .Where(x => !hostedRooms.Any(y => y.RoomId == x.RoomId))
    }
}
