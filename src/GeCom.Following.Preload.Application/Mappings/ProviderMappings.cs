using GeCom.Following.Preload.Contracts.Preload.Providers;
using GeCom.Following.Preload.Domain.Preloads.Providers;

namespace GeCom.Following.Preload.Application.Mappings;

internal static class ProviderMappings
{
    public static ProviderResponse ToResponse(Provider provider)
    {
        var result = new ProviderResponse(
            provider.ProvId,
            provider.Cuit,
            provider.RazonSocial,
            provider.Mail,
            provider.FechaCreacion,
            provider.FechaBaja
        );

        return result;
    }
}

