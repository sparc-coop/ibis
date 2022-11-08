using Microsoft.Extensions.DependencyInjection;

namespace Sparc.Ibis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIbis(this IServiceCollection services)
    {
        services.AddScoped<IbisTranslator>();
        return services;
    }
}
