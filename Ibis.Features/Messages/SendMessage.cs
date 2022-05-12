using Microsoft.AspNetCore.SignalR;
using Sparc.Notifications.Twilio;

namespace Ibis.Features.Messages;

public record SendMessageRequest(string RoomId, string? Message, string Language, string? MessageId, string? ModifiedMessage, byte[]? Bytes);
public class SendMessage : Feature<SendMessageRequest, Message>
{
    public SendMessage(IRepository<Message> messages,
        IRepository<Room> rooms,
        IRepository<User> users,
        IHubContext<RoomHub> room,
        IbisEngine ibisEngine,
        TwilioService twilio)
    {
        Messages = messages;
        Rooms = rooms;
        Users = users;
        Room = room;
        IbisEngine = ibisEngine;
        Twilio = twilio;
    }

    public IRepository<Message> Messages { get; }
    public IRepository<Room> Rooms { get; }
    public IRepository<User> Users { get; }
    public IHubContext<RoomHub> Room { get; }
    public IbisEngine IbisEngine { get; }
    public TwilioService Twilio { get; }

    public override async Task<Message> ExecuteAsync(SendMessageRequest request)
    {
        var user = await Users.FindAsync(User.Id());
        Message message;
        Room room;

        // Message from text input
        if (request.MessageId == null)
        {
            message = new Message(request.RoomId, User.Id(), request.Language ?? user!.PrimaryLanguageId, SourceTypes.Text, user.FullName, user.Initials);
            message.UserName = user.FullName;
            message.SetText(request.Message);
            message.Color = user.Color;

            // Translate and Speak
            room = await Rooms.FindAsync(request.RoomId);
            room.LastActiveDate = DateTime.UtcNow;
            await IbisEngine.TranslateAsync(message, room!.Languages);
            await IbisEngine.SpeakAsync(message);
            await Messages.AddAsync(message);
        }
        // Message from Upload
        else if (request.MessageId != null && request.ModifiedMessage == null)
        {
            message = await Messages.FindAsync(request.MessageId);
            message.SetModifiedText(request.ModifiedMessage);

            // Translate, Speak is Audio from Upload
            room = await Rooms.FindAsync(request.RoomId);
            room.LastActiveDate = DateTime.UtcNow;
            await IbisEngine.UploadAudioToStorage(message, request.Bytes);
            await IbisEngine.TranslateAsync(message, room!.Languages);
            await Messages.UpdateAsync(message);
        }
        // Message from Upload AND modified
        else
        {
            message = await Messages.FindAsync(request.MessageId);
            message.SetModifiedText(request.ModifiedMessage);

            // Translate modified message and speak
            room = await Rooms.FindAsync(request.RoomId);
            room.LastActiveDate = DateTime.UtcNow;
            await IbisEngine.UploadAudioToStorage(message, request.Bytes);
            await IbisEngine.TranslateAsync(message, room!.Languages);
            await IbisEngine.SpeakAsync(message);
            await Messages.UpdateAsync(message);
        }

        await Rooms.UpdateAsync(room);
        await Room.Clients.Group($"{request.RoomId}").SendAsync("NewMessage", message);

        var usersToSms = room.ActiveUsers.Where(x => x.PhoneNumber != null && message.HasTranslation(x.Language)).ToList();
        foreach (var userToSms in usersToSms)
            await Twilio.SendSmsAsync(userToSms.PhoneNumber!, message.GetTranslation(userToSms.Language));

        return message;
    }
}
