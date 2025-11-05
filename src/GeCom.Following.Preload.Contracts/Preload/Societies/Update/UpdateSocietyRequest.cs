namespace GeCom.Following.Preload.Contracts.Preload.Societies.Update;

/// <summary>
/// Request DTO for updating a society.
/// </summary>
public sealed record UpdateSocietyRequest(
    int SocId,
    string Codigo,
    string Descripcion,
    string Cuit,
    bool? EsPrecarga
);
