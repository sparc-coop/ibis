namespace Ibis.Messages;

public class MessageTag(string key, string value, bool translate)
{
    public string Key { get; private set; } = key;
    public string Value { get; set; } = value;
    public bool Translate { get; private set; } = translate;
}
