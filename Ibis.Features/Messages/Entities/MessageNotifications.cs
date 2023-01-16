namespace Ibis.Messages;

public record MessageAudioChanged(Message Message) : Notification(Message.RoomId + "|" + Message.Language);
public record MessageTextChanged(Message Message) : Notification(Message.RoomId + "|" + Message.Language);
public record MessageDeleted(Message Message) : Notification(Message.RoomId + "|" + Message.Language);
