using Ibis.Features.Sparc.Realtime;

namespace Ibis.Features.Messages;

public record MessageTextChanged(Message Message) : RoomNotification(Message.RoomId);
public class SpeakMessage : BackgroundFeature<MessageTextChanged>
{
    public SpeakMessage(ISpeaker synthesizer, IRepository<Message> messages)
    {
        Synthesizer = synthesizer;
        Messages = messages;
    }

    public ISpeaker Synthesizer { get; }
    public IRepository<Message> Messages { get; }

    public override async Task ExecuteAsync(MessageTextChanged notification)
    {
        await notification.Message.SpeakAsync(Synthesizer);
        await Messages.UpdateAsync(notification.Message);
    }
}
