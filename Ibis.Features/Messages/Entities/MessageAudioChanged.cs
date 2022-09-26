using Ibis.Features.Sparc.Realtime;

namespace Ibis.Features.Messages;

public record MessageAudioChanged(Message Message) : SparcNotification(Message.RoomId);
