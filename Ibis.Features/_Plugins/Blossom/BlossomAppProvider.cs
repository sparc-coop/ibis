using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ibis._Plugins.Blossom;

public class BlossomAppProvider
{
    public Type App { get; private set; } = null!;
    public RenderMode RenderMode { get; private set; } = RenderMode.WebAssembly;

    public void SetApp<T>(RenderMode renderMode)
    {
        App = typeof(T);
        RenderMode = renderMode;
    }
}
