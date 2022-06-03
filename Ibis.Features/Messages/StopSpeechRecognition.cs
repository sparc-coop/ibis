namespace Ibis.Features.Messages;

public class StopSpeechRecognition : PublicFeature<string, User>
{
    public IRepository<User> Users { get; }
    public IbisEngine IbisEngine { get; }

    public StopSpeechRecognition(IRepository<User> users, IbisEngine ibisEngine)
    {
        Users = users;
        IbisEngine = ibisEngine;
    }
    public async override Task<User> ExecuteAsync(string userId)
    {
        var user = await Users.FindAsync(userId);
        if (user != null)
        {
            user.SetStopRecognizingSpeech(true);
            await Users.UpdateAsync(user);
            return user;
        }
        return user;
    }
}
