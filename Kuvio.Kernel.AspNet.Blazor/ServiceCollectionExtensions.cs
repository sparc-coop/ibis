using Kuvio.Kernel.AspNet.Blazor.Modal;
using Kuvio.Kernel.AspNet.Blazor.Toast.Configuration;
using Kuvio.Kernel.AspNet.Blazor.Toast.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kuvio.Kernel.AspNet.Blazor
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlazorModal(this IServiceCollection services)
        {
            return services.AddScoped<IModalService, ModalService>();
        }

        public static IServiceCollection AddBlazorToast(this IServiceCollection services)
        {
            return AddBlazorToast(services, new ToastOptions());
        }

        public static IServiceCollection AddBlazorToast(this IServiceCollection services, Action<ToastOptions> toastOptionsAction)
        {
            if (toastOptionsAction == null) throw new ArgumentNullException(nameof(toastOptionsAction));

            var toastOptions = new ToastOptions();
            toastOptionsAction(toastOptions);

            return AddBlazorToast(services, toastOptions);
        }
        public static IServiceCollection AddBlazorToast(this IServiceCollection services, ToastOptions toastOptions)
        {
            if (toastOptions == null) throw new ArgumentNullException(nameof(toastOptions));

            services.TryAddScoped<IToastService>(builder => new ToastService(toastOptions));

            return services;
        }
    }
}