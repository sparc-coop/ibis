using Microsoft.AspNetCore.Components;

namespace Sparc.Ibis;

public record IbisContent(string Tag, string Text, string Language, string? Audio, DateTime Timestamp)
{
    public override string ToString() => Text;

    public MarkupString Html => new(Text);
}
