//using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Components.Rendering;
//using Microsoft.AspNetCore.Components.RenderTree;
//using Microsoft.AspNetCore.SignalR.Protocol;

//#pragma warning disable BL0006 // Do not use RenderTree types
//namespace Ibis.Web.Shared;

//// Work in progress -- idea for automatic translation via the Blazor engine
//public class IbisTranslator : ComponentBase
//{
//    public IbisTranslator(IbisContentProvider ibis)
//    {
//        Ibis = ibis;
//    }

//    [Parameter] public RenderFragment? ChildContent { get; set; }
//    public IbisContentProvider Ibis { get; }

//    protected override void BuildRenderTree(RenderTreeBuilder builder)
//    {
//        base.BuildRenderTree(builder);

//        builder.AddContent(0, ChildContent);

//        // Replay the content
//        var frames = builder.GetFrames().Array;

//        for (var i = 0; i < frames.Length; i++)
//        {
//            var frame = frames[i];

//            if (frame.FrameType == RenderTreeFrameType.Text)
//            {
//                var text = frame.TextContent;
//                var translated = Ibis.Translate(text);
//                frames[i] = new RenderTreeFrame(frame.Sequence, frame.ElementSubtreeLength, frame.ElementSubtreeLength, RenderTreeFrameType.Text, translated);
//            }
//        }
//        {
//            if (frame.FrameType == Microsoft.AspNetCore.Components.RenderTree.RenderTreeFrameType.Text)
//                frames[frames.IndexOf(frame)]
//        }
//    }

//    static string Translate(string text)
//    {
//        return text;
//    }
//}
