namespace Ibis.Features.Rooms;

public record GetRoomResponse
{
    public string RoomId { get; set; }
    public string RoomType { get; set; }
    public string Slug { get; set; }
    public DateTime? LastActiveDate { get; set; }
    public DateTime StartDate { get; private set; }
    public string Name { get; private set; }
    public List<InvitedUser>? Users { get; set; }

    public GetRoomResponse(Room room)
    {
        RoomId = room.Id;
        RoomType = room.RoomType;
        LastActiveDate = room.LastActiveDate;
        StartDate = room.StartDate;
        Name = room.Name;
        Slug = room.Slug;
        Users = room.Users;
    }
}
