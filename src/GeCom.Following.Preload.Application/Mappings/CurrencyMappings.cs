using GeCom.Following.Preload.Contracts.Preload.Currencies;
using GeCom.Following.Preload.Domain.Preloads.Currencies;

namespace GeCom.Following.Preload.Application.Mappings;

internal static class CurrencyMappings
{
    public static CurrencyResponse ToResponse(Currency currency)
    {
        CurrencyResponse result = new(
            currency.MonedaId,
            currency.Codigo,
            currency.Descripcion,
            currency.CodigoAfip
        );

        return result;
    }
}

