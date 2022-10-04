namespace Ibis.Features.Messages;

public record MessageAudioChanged(Message Message) : SparcNotification(Message.Id);