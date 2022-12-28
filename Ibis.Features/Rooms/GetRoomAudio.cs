namespace Ibis.Features.Rooms;

public partial class Rooms
{
    public async Task<AudioMessage?> GetAudioAsync(string roomId, string language, ISpeaker speaker)
    {
        var room = await GetAsync(roomId);

        var roomMessages = await Messages.Query
            .Where(x => x.RoomId == roomId && x.Language == language && x.Audio != null)
            .OrderBy(x => x.Timestamp)
            .ToListAsync();

        await room!.SpeakAsync(speaker, roomMessages);

        return room.Audio;
    }
}
