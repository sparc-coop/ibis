﻿namespace Ibis.Messages;

public record DeleteMessageRequest(string RoomId, string MessageId);
public class DeleteMessage : Feature<DeleteMessageRequest, bool>
{
    public IRepository<Room> Rooms { get; }
    public IRepository<Message> Messages { get; }

    public DeleteMessage(IRepository<Room> rooms, IRepository<Message> messages)
    {
        Rooms = rooms;
        Messages = messages;
    }

    public override async Task<bool> ExecuteAsync(DeleteMessageRequest request)
    {
        var message = await Messages.FindAsync(request.MessageId);
        if (message == null || message.User.Id != User.Id()) 
            return false;

        // Delete all translations
        foreach (var translation in message.Translations)
        {
            var translatedMessage = await Messages.FindAsync(translation.SourceMessageId);
            if (translatedMessage != null)
                await Messages.ExecuteAsync(translatedMessage, x => x.Delete());
        }

        await Messages.ExecuteAsync(message, x => x.Delete());
        return true;
    }
}
