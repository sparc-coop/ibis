namespace Ibis.Features.Messages;

public record MessageTextChanged(string RoomId, string MessageId) : INotification;
public class SpeakMessage : BackgroundFeature<MessageTextChanged>
{
    public SpeakMessage(ISynthesizer synthesizer, IRepository<Message> messages)
    {
        Synthesizer = synthesizer;
        Messages = messages;
    }

    public ISynthesizer Synthesizer { get; }
    public IRepository<Message> Messages { get; }

    public override async Task ExecuteAsync(MessageTextChanged notification)
    {
        await Messages.ExecuteAsync(notification.MessageId, async message => await message.SpeakAsync(Synthesizer));
    }
}
