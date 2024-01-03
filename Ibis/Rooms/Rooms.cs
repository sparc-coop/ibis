using Ardalis.Specification;

namespace Ibis.Rooms;

public class Rooms : BlossomRoot<Room>
{
    public Rooms(IRepository<Room> rooms) : base(rooms)
    {
        Api.GetAllAsync = (User user) => MyRooms(rooms, user, "Chat");
        Api.DeleteAsync = (Room r) => r.Close();
    }

    public IQueryable<Room> MyRooms(IRepository<Room> rooms, User user, string roomType) =>
        rooms.Query
           .Where(x => x.HostUser.Id == user.Id
               && (roomType != null ? x.RoomType == roomType : x.RoomType.Any())
               && x.EndDate == null)
           .OrderByDescending(x => x.LastActiveDate);
}


public record UserJoined(string RoomId, UserAvatar User) : Notification(RoomId);
public record UserLeft(string RoomId, UserAvatar User) : Notification(RoomId);
public record FileUploaded(string RoomId, string Url) : Notification(RoomId);
