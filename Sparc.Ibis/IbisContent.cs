using Microsoft.AspNetCore.Components;

namespace Sparc.Ibis;

public record IbisContent(string Tag, string Text, string Language, string? Audio, DateTime Timestamp, Dictionary<string, string>? Tags)
{
    public override string ToString() => Text;

    public MarkupString Html => new(Text);

    public string? this[string tag] => Tags?.ContainsKey(tag) == true ? Tags[tag] : null;
}
