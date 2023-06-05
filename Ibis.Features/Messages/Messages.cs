namespace Ibis.Messages;

public class Messages : BlossomAggregate<Message>
{
    public Messages()
    {
        DeleteAsync = (Message m, User user) => m.Delete(user);
        UpdateAsync = (Message m, string text) => m.SetText(text);
    }
}
