using Microsoft.AspNetCore.Mvc;

namespace Ibis.Rooms
{
    public record DownloadAudioRequest(string RoomId, string Language);

    public class DownloadAudioContent : Feature<DownloadAudioRequest, string?>
    {
        public DownloadAudioContent(IRepository<Room> rooms, IRepository<Message> messages, ISpeaker speaker, GetAllContent getAllContent)
        {
            Rooms = rooms;
            Messages = messages;
            Speaker = speaker;
            GetAllContent = getAllContent;
        }

        public IRepository<Room> Rooms { get; }
        public IRepository<Message> Messages { get; }
        public ISpeaker Speaker { get; }

        public GetAllContent GetAllContent { get; }

        public async override Task<string?> ExecuteAsync([FromQuery]DownloadAudioRequest request)
        {
            var room = await Rooms.FindAsync(request.RoomId);


            var content = await GetAllContent.ExecuteAsync(new(request.RoomId, request.Language));

            await room!.SpeakAsync(Speaker, content.Content);

            return room.Audio?.Url;
        }
    }
}
