using Kuvio.Kernel.AspNet.Blazor.Toast.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Kuvio.Kernel.AspNet.Blazor.Toast
{
    public class ToastsBase : ComponentBase
    {
        [Inject] private IToastService ToastService { get; set; }

        protected string Css { get; set; } = string.Empty;
        internal List<Toast> ToastList { get; set; } = new List<Toast>();

        protected override void OnInitialized()
        {
            ToastService.OnShow += ShowToast;
            Css = $"position-{ToastService.ToastOptions.Position.ToString().ToLower()}";
        }
        public async Task RemoveToast(Guid toastId)
        {
            await InvokeAsync(() =>
            {
                var toastInstance = ToastList.SingleOrDefault(x => x.Id == toastId);
                ToastList.Remove(toastInstance);
                StateHasChanged();
            });
        }

        private ToastSettings BuildToastSettings(ToastLevel level, string message, string heading)
        {
            switch (level)
            {
                case ToastLevel.Info:
                    return new ToastSettings(string.IsNullOrWhiteSpace(heading) ? "Info" : heading, message, "blazor-toast-info", "");

                case ToastLevel.Success:
                    return new ToastSettings(string.IsNullOrWhiteSpace(heading) ? "Success" : heading, message, "blazor-toast-success", "");

                case ToastLevel.Warning:
                    return new ToastSettings(string.IsNullOrWhiteSpace(heading) ? "Warning" : heading, message, "blazor-toast-warning", "");

                case ToastLevel.Error:
                    return new ToastSettings(string.IsNullOrWhiteSpace(heading) ? "Error" : heading, message, "blazor-toast-error", "");
            }

            throw new InvalidOperationException();
        }

        private void ShowToast(ToastLevel level, string message, string heading)
        {
            var settings = BuildToastSettings(level, message, heading);
            var options = ToastService.ToastOptions;
            var toast = new Toast
            {
                Id = Guid.NewGuid(),
                TimeStamp = DateTime.Now,
                ToastSettings = settings,
                Options = options
            };

            ToastList.Add(toast);

            var timeout = options.Timeout * 1000;
            var toastTimer = new Timer(timeout);
            toastTimer.Elapsed += (sender, args) => { RemoveToast(toast.Id); };
            toastTimer.AutoReset = false;
            toastTimer.Start();

            StateHasChanged();
        }
    }
}