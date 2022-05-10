namespace Ibis.Features.Messages;

public record UploadFileRequest(string RoomId, string Language, byte[] Bytes, string FileName);

public class UploadFile : PublicFeature<UploadFileRequest, Message>
{
    public IbisEngine IbisEngine { get; }
    public IRepository<Message> Messages { get; }
    public IRepository<User> Users { get; }

    public UploadFile(IbisEngine ibisEngine, IRepository<User> users, IRepository<Message> messages)
    {
        IbisEngine = ibisEngine;
        Users = users;
        Messages = messages;
    }

    public async override Task<Message> ExecuteAsync(UploadFileRequest request)
    {
        var user = await Users.FindAsync(User.Id());
        var message = new Message(request.RoomId, User.Id(), request.Language ?? user!.PrimaryLanguageId, SourceTypes.Upload, user.FullName, user.Initials);

        message = await IbisEngine.TranscribeSpeechFromFile(message, request.Bytes, request.FileName);
        await Messages.AddAsync(message);

        return message;
    }
}
