namespace Ibis.Features.Rooms;

public record GetRoomAudioRequest(IEnumerable<string> Files, string RoomId, string Language);
public class GetRoomAudio : Feature<GetRoomAudioRequest, AudioMessage?>
{
    public GetRoomAudio(IRepository<Room> rooms, IRepository<Message> messages, ISpeaker speaker)
    {
        Rooms = rooms;
        Messages = messages;
        Speaker = speaker;
    }

    public IRepository<Room> Rooms { get; }
    public IRepository<Message> Messages { get; }
    public ISpeaker Speaker { get; }

    public async override Task<AudioMessage?> ExecuteAsync(GetRoomAudioRequest request)
    {
        var room = await Rooms.FindAsync(request.RoomId);

        var messages = await Messages.Query
            .Where(x => x.RoomId == request.RoomId && x.Language == request.Language && x.Audio != null)
            .OrderBy(x => x.Timestamp)
            .ToListAsync();

        await room!.SpeakAsync(Speaker, messages);

        return room.Audio;
    }
}
