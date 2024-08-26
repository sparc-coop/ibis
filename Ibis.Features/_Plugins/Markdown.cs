using Markdig;
using Markdig.Renderers;

namespace Ibis._Plugins
{
    public static class MarkdownExtensions
    {
        public static string ToHtml(string markdown)
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseEmphasisExtras()
                .Build();

            var writer = new StringWriter();
            var renderer = new HtmlRenderer(writer);
            renderer.ImplicitParagraph = true; //This is needed to render a single line of text without a paragraph tag
            pipeline.Setup(renderer);

            renderer.Render(Markdown.Parse(markdown, pipeline));
            writer.Flush();

            return writer.ToString();
        }       
    }
}
