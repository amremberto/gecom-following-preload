using Microsoft.Extensions.DependencyInjection;

namespace GeCom.Following.Preload.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddPreloadInfrastructure(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register infrastructure services here

        return services;
    }
}
