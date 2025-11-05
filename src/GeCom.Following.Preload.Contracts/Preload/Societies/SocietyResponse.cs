namespace GeCom.Following.Preload.Contracts.Preload.Societies;

/// <summary>
/// Response DTO for Society.
/// </summary>
public sealed record SocietyResponse(
    int SocId,
    string Codigo,
    string Descripcion,
    string Cuit,
    DateTime FechaCreacion,
    DateTime? FechaBaja,
    bool? EsPrecarga
);
