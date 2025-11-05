namespace GeCom.Following.Preload.Contracts.Preload.Societies.Create;

/// <summary>
/// Request DTO for creating a society.
/// </summary>
public sealed record CreateSocietyRequest(
    string Codigo,
    string Descripcion,
    string Cuit,
    bool? EsPrecarga = null
);
