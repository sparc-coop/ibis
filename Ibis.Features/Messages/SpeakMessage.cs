using Microsoft.AspNetCore.SignalR;

namespace Ibis.Features.Messages;

public class SpeakMessage : BackgroundFeature<Message>
{
    public SpeakMessage(ISynthesizer synthesizer, IRepository<Room> rooms, IHubContext<RoomHub> hub)
    {
        Synthesizer = synthesizer;
        Rooms = rooms;
        Hub = hub;
    }

    public ISynthesizer Synthesizer { get; }
    public IRepository<Room> Rooms { get; }
    public IHubContext<RoomHub> Hub { get; }

    public override async Task ExecuteAsync(Message message)
    {
        var room = await Rooms.FindAsync(message.RoomId);

        if (!string.IsNullOrWhiteSpace(message.Text) && string.IsNullOrWhiteSpace(message.AudioUrl))
            await message.SpeakAsync(Synthesizer);
        
        // var translatedMessages = 
        // await Synthesizer.TranslateAsync(message, room)
    }
}
