namespace GeCom.Following.Preload.Contracts.Preload.Currencies.Create;

/// <summary>
/// Request DTO for creating a currency.
/// </summary>
public sealed record CreateCurrencyRequest(
    string Codigo,
    string Descripcion,
    string? CodigoAfip = null
);

