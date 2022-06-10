namespace Ibis.Features;

public class StopSpeechRecognition : PublicFeature<Message, bool>
{
    public IRepository<User> Users { get; }
    public IbisEngine IbisEngine { get; }

    public StopSpeechRecognition(IRepository<User> users, IbisEngine ibisEngine)
    {
        Users = users;
        IbisEngine = ibisEngine;
    }
    public async override Task<bool> ExecuteAsync(Message message)
    {
        //var user = await Users.FindAsync(message.UserId);
        //if (user != null)
        //{
        //    user.SetStopRecognizingSpeech(true);
        //    await Users.UpdateAsync(user);
        //    return user;
        //}
        message = await IbisEngine.ContinuousSpeechRecognitionAsync(message, message.UserId, false);
        var test = message;
        return true;
    }
}
