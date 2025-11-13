namespace GeCom.Following.Preload.Contracts.Preload.Currencies.Update;

/// <summary>
/// Request DTO for updating a currency.
/// </summary>
public sealed record UpdateCurrencyRequest(
    int MonedaId,
    string Codigo,
    string Descripcion,
    string? CodigoAfip
);

