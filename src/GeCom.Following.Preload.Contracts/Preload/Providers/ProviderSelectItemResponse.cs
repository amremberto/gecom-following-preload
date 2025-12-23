namespace GeCom.Following.Preload.Contracts.Preload.Providers;

/// <summary>
/// DTO for Provider select dropdown items.
/// </summary>
public sealed record ProviderSelectItemResponse(
    string Cuit,
    string RazonSocial
);

