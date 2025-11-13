namespace GeCom.Following.Preload.Contracts.Preload.DocumentTypes.Update;

/// <summary>
/// Request DTO for updating an existing document type.
/// </summary>
public sealed record UpdateDocumentTypeRequest(
    string Descripcion,
    string? Letra,
    string Codigo,
    string? DescripcionLarga,
    bool IsFec
);

