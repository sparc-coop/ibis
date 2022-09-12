using Ibis.Features.Sparc.Realtime;

namespace Ibis.Features.Messages;

public class HearMessage : Feature<string>
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

    public async override Task<string> ExecuteAsync()
    {
        return await Listener.BeginListeningAsync();
        
    }
}
