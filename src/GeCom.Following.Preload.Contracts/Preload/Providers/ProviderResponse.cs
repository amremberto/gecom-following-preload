namespace GeCom.Following.Preload.Contracts.Preload.Providers;

/// <summary>
/// Response DTO for Provider.
/// </summary>
public sealed record ProviderResponse(
    int ProvId,
    string Cuit,
    string RazonSocial,
    string Mail,
    DateTime FechaCreacion,
    DateTime? FechaBaja
);

