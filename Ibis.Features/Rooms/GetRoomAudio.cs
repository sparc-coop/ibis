using Ibis.Features.Messages.Queries;

namespace Ibis.Features.Rooms;

public partial class Rooms
{
    public async Task<AudioMessage?> GetAudioAsync(Room room, string language, ISpeaker speaker)
    {
        var roomMessages = await Messages.GetAllAsync(new MessagesForRoom(room.Id, language, true));
        await room!.SpeakAsync(speaker, roomMessages);
        return room.Audio;
    }
}
