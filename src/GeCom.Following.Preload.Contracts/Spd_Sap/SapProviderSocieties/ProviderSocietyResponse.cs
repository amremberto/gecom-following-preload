namespace GeCom.Following.Preload.Contracts.Spd_Sap.SapProviderSocieties;

/// <summary>
/// Response DTO for provider society relationship.
/// </summary>
public sealed record ProviderSocietyResponse(
    string Cuit,
    string? Name
);
