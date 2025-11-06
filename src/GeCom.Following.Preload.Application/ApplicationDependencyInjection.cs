using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace GeCom.Following.Preload.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddPreloadApplication(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        Assembly assembly = typeof(ApplicationAssembly).Assembly;

        // Add MediatR (register handlers from this assembly)
        services.AddMediatR(cfg
            => cfg.RegisterServicesFromAssembly(assembly));

        // FluentValidation (validators from this assembly; includes internal validators)
        // Requires pacakge: FluentValidation.DependencyInjectionExtensions
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}
