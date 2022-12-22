using Microsoft.Extensions.DependencyInjection;

namespace Sparc.Ibis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIbis(this IServiceCollection services, string apiBaseUrl = "https://ibis.chat/")
    {
        var client = services.AddHttpClient<IbisClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestVersion = new Version(2, 0);
        });

        services.AddScoped<IbisTranslator>();
        return services;
    }
}
