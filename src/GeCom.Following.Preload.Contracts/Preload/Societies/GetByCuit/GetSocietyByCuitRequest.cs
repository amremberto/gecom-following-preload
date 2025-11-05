namespace GeCom.Following.Preload.Contracts.Preload.Societies.GetByCuit;

/// <summary>
/// Request DTO for getting a society by CUIT.
/// </summary>
public sealed record GetSocietyByCuitRequest(string Cuit);
