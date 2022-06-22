namespace Ibis.Features.Messages;

public record TranscribeSpeechRequest(string RoomId, string Language);
public class TranscribeSpeech : PublicFeature<TranscribeSpeechRequest, Message>
{
    public IRepository<Message> Messages { get; }
    public IRepository<User> Users { get; }
    public IbisEngine IbisEngine { get; }

    public TranscribeSpeech(IRepository<Message> messages, IRepository<User> users, IbisEngine ibisEngine)
    {
        Messages = messages;
        Users = users;
        IbisEngine = ibisEngine;
    }

    public async override Task<Message> ExecuteAsync(TranscribeSpeechRequest request)
    {
        var user = await Users.FindAsync(User.Id());
        var message = new Message(request.RoomId, User.Id(), request.Language ?? user!.PrimaryLanguageId, SourceTypes.Microphone, user.FullName, user.Initials);
        message = await IbisEngine.TranscribeSpeechFromMic(message);
        if (message.Text == null || message.Text.Length == 0)
        {
            await Messages.DeleteAsync(message);
        }
        else
        {
            await Messages.AddAsync(message);
        }
        return message;
    }
}
