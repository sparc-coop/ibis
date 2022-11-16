namespace Ibis.Features.Messages;

public class SpeakMessage : RealtimeFeature<MessageTextChanged>
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
