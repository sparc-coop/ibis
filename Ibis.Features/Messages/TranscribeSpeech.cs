using Ibis.Features.Sparc.Realtime;

namespace Ibis.Features.Messages;

public record TranscribeSpeechRequest(string RoomId, string Language);
public class TranscribeMessage : PublicFeature<TranscribeSpeechRequest, string>
{
    public IRepository<Message> Messages { get; }
    public IRepository<User> Users { get; }
    public IListener Listener { get; }

    public TranscribeMessage(IRepository<Message> messages, IRepository<User> users, IListener listener)
    {
        Messages = messages;
        Users = users;
        Listener = listener;
    }

    public async override Task<string> ExecuteAsync(TranscribeSpeechRequest request)
    {
        return await Listener.BeginListeningAsync();
        
    }
}
