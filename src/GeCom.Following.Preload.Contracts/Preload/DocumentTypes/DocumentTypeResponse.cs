namespace GeCom.Following.Preload.Contracts.Preload.DocumentTypes;

/// <summary>
/// Response DTO for DocumentType.
/// </summary>
public sealed record DocumentTypeResponse(
    int TipoDocId,
    string Descripcion,
    string? Letra,
    string Codigo,
    string? DescripcionLarga,
    DateTime FechaCreacion,
    DateTime? FechaBaja,
    bool IsFec
);

