namespace Ibis.Messages;

public record HearMessageResponse(string SessionId);
public class HearMessage : Feature<HearMessageResponse>
{
    public IRepository<Message> Messages { get; }
    public IRepository<User> Users { get; }
    public IListener Listener { get; }

    public HearMessage(IRepository<Message> messages, IRepository<User> users, IListener listener)
    {
        Messages = messages;
        Users = users;
        Listener = listener;
    }

    public async override Task<HearMessageResponse> ExecuteAsync()
    {
        var result = await Listener.BeginListeningAsync();
        return new(result);
        
    }
}
