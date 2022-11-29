using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace Sparc.Ibis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIbis(this IServiceCollection services)
    {
        services.AddScoped<IbisTranslator>();
        return services;
    }

    public static IApplicationBuilder UseIbis(this IApplicationBuilder app, string[]? supportedCultures = null)
    {
        supportedCultures ??= CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Select(x => x.Name)
            .ToArray();

        app.UseRequestLocalization(options => options
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures));

        return app;
    }
}
