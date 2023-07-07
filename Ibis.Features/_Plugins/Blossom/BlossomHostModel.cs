using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ibis._Plugins.Blossom;

public class BlossomHostModel
{
    public Type App { get; set; }
    public RenderMode RenderMode { get; set; }

    public BlossomHostModel(BlossomAppProvider appProvider)
    {
        App = appProvider.App;
        RenderMode = appProvider.RenderMode;
    }
}
