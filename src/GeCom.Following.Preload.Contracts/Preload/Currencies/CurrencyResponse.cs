namespace GeCom.Following.Preload.Contracts.Preload.Currencies;

/// <summary>
/// Response DTO for Currency.
/// </summary>
public sealed record CurrencyResponse(
    int MonedaId,
    string Codigo,
    string Descripcion,
    string? CodigoAfip
);

