namespace GeCom.Following.Preload.Contracts.Preload.Societies;

/// <summary>
/// List item DTO for Society.
/// </summary>
public sealed record SocietyListItem(
    int SocId,
    string Codigo,
    string Descripcion,
    string Cuit,
    bool? EsPrecarga
);
