namespace Ibis.Features.Messages;

public record UploadFileRequest(string RoomId, string Language, byte[] Bytes, string FileName);

public class UploadFile : PublicFeature<UploadFileRequest, Message>
{
    public IbisEngine IbisEngine { get; }
    public IRepository<Message> Messages { get; }
    public IRepository<User> Users { get; }
    public IRepository<Room> Rooms { get; }

    public UploadFile(IbisEngine ibisEngine, IRepository<User> users, IRepository<Message> messages, IRepository<Room> rooms)
    {
        IbisEngine = ibisEngine;
        Users = users;
        Messages = messages;
        Rooms = rooms;
    }

    public async override Task<Message> ExecuteAsync(UploadFileRequest request)
    {
        var user = await Users.FindAsync(User.Id());
        var message = new Message(request.RoomId, User.Id(), request.Language ?? user!.PrimaryLanguageId, SourceTypes.Upload, user.FullName, user.Initials);
        var room = await Rooms.FindAsync(request.RoomId);

        var subroom = new Room(room!, message);
        message.SetSubroomId(subroom.Id);
        await Rooms.UpdateAsync(subroom);

        var messages = await IbisEngine.TranscribeSpeechFromFile(message, request.Bytes, request.FileName);
        foreach (var subMessage in messages)
            await Messages.AddAsync(subMessage);

        return message;
    }
}
