using Kuvio.Kernel.AspNet.Blazor.Toast.Configuration;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Kuvio.Kernel.AspNet.Blazor.Toast
{
    public class BlazorToastBase : ComponentBase
    {
        [Parameter] public Guid ToastId { get; set; }
        [Parameter] public ToastSettings ToastSettings { get; set; }
        [Parameter] public ToastOptions ToastOptions { get; set; }
        [CascadingParameter] private BlazorToasts ToastsContainer { get; set; }

        protected async Task Close()
        {
            await ToastsContainer.RemoveToast(ToastId);
        }
    }
}
