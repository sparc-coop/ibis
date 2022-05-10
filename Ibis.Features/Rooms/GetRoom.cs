namespace Ibis.Features.Rooms;

public record GetRoomRequest(string RoomId, string Language);
public record GetRoomResponse(Room Room, List<Message> Messages);
public class GetRoom : PublicFeature<GetRoomRequest, GetRoomResponse>
{
    public GetRoom(IRepository<Room> rooms, IRepository<Message> messages, IbisEngine ibisEngine)
    {
        Rooms = rooms;
        Messages = messages;
        IbisEngine = ibisEngine;
    }

    public IRepository<Room> Rooms { get; }
    public IRepository<Message> Messages { get; }
    public IbisEngine IbisEngine { get; }

    public async override Task<GetRoomResponse> ExecuteAsync(GetRoomRequest request)
    {
        var room = await Rooms.FindAsync(request.RoomId);
        if (room == null)
            throw new NotFoundException($"Room {request.RoomId} not found!");

        if (!room.Languages.Any(x => x.Name == request.Language))
        {
            room.AddLanguage(request.Language);
            await Rooms.UpdateAsync(room);
        }

        var messages = Messages.Query
            .Where(x => x.RoomId == room.Id)
            .OrderBy(x => x.Timestamp)
            .ToList();

        var untranslatedMessages = messages.Where(x => !x.HasTranslation(request.Language)).ToList();
        foreach (var message in untranslatedMessages)
        {
            await IbisEngine.TranslateAsync(message, request.Language);
            await Messages.UpdateAsync(message);
        }

        return new(room, messages);
    }
}
