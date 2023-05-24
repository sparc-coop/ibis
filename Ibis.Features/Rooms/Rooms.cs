using Ibis.Rooms.Queries;

namespace Ibis.Rooms;

public class Rooms : BlossomAggregate<Room>
{
    public Rooms() : base()
    {
        GetAllAsync = async (User user, IRepository<Room> rooms) => await rooms.GetAllAsync(new MyRooms(user, "Chat"));
        DeleteAsync = (Room r) => r.Close();
    }
}


public record SourceMessage(string RoomId, string MessageId);
public record UserJoined(string RoomId, UserAvatar User) : Notification(RoomId);
public record UserLeft(string RoomId, UserAvatar User) : Notification(RoomId);