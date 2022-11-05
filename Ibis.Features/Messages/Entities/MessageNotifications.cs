﻿namespace Ibis.Features.Messages;

public record MessageAudioChanged(Message Message) : Notification(Message.RoomId + "|" + Message.Language);
public record MessageTextChanged(Message Message) : Notification(Message.RoomId + "|" + Message.Language);
