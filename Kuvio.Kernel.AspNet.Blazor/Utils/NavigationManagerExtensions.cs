using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kuvio.Kernel.AspNet.Blazor.Utils
{
    public static class NavigationManagerExtensions
    {
        public static string GetRelativeUri(this NavigationManager navigationManager)
        {
            if (navigationManager == null)
            {
                return null;
            }

            return navigationManager.Uri.Replace(navigationManager.BaseUri, "");
        }
    }
}