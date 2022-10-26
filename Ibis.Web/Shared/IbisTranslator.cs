using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

#pragma warning disable BL0006 // Do not use RenderTree types
namespace Ibis.Web.Shared;

// Work in progress -- idea for automatic translation via the Blazor engine
public class IbisTranslator : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);

        builder.AddContent(0, ChildContent);

        // Replay the content
        var frames = builder.GetFrames().Array
            .OrderBy(x => x.Sequence)
            .ToArray();

        builder.Clear();

        foreach (var frame in frames)
        {
            if (frame.FrameType == Microsoft.AspNetCore.Components.RenderTree.RenderTreeFrameType.Text)
                builder.AddContent(frame.Sequence, Translate(frame.TextContent));
        }
    }

    static string Translate(string text)
    {
        return text;
    }
}
