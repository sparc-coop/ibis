using Ibis.Features.Sparc.Realtime;

namespace Ibis.Features.Rooms;

public record GetRoomRequest(string RoomId);
public record GetRoomResponse
{
    public string RoomId { get; set; }
    public DateTime? LastActiveDate { get; set; }
    public DateTime StartDate { get; private set; }
    public string Name { get; private set; }
    public List<UserSummary>? ActiveUsers { get; set; }
    public List<UserSummary>? PendingUsers { get; set; }

    public GetRoomResponse(Room room)
    {
        RoomId = room.Id;
        LastActiveDate = room.LastActiveDate;
        StartDate = room.StartDate;
        Name = room.Name;
        ActiveUsers = room.ActiveUsers;
        PendingUsers = room.PendingUsers;
    }
}

public class GetRoom : PublicFeature<GetRoomRequest, GetRoomResponse>
{
    public GetRoom(IRepository<Room> rooms, IRepository<Message> messages, ITranslator translator)
    {
        Rooms = rooms;
        Messages = messages;
        Translator = translator;
    }

    public IRepository<Room> Rooms { get; }
    public IRepository<Message> Messages { get; }
    public ITranslator Translator { get; }

    public async override Task<GetRoomResponse> ExecuteAsync(GetRoomRequest request)
    {
        var room = await Rooms.FindAsync(request.RoomId);
        if (room == null)
            throw new NotFoundException($"Room {request.RoomId} not found!");
       
        return new(room);
    }
}
