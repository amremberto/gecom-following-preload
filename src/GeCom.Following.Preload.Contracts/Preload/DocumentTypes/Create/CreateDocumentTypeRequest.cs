namespace GeCom.Following.Preload.Contracts.Preload.DocumentTypes.Create;

/// <summary>
/// Request DTO for creating a new document type.
/// </summary>
public sealed record CreateDocumentTypeRequest(
    string Descripcion,
    string? Letra,
    string Codigo,
    string? DescripcionLarga,
    bool IsFec
);

