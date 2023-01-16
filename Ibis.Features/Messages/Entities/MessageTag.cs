namespace Ibis.Messages;

public class MessageTag
{
    public MessageTag(string key, string value, bool translate)
    {
        Key = key;
        Value = value;
        Translate = translate;
    }

    public string Key { get; private set; }
    public string Value { get; set; }
    public bool Translate { get; private set; }
}
