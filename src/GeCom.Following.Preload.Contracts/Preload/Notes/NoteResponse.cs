namespace GeCom.Following.Preload.Contracts.Preload.Notes;

/// <summary>
/// Response DTO for Note.
/// </summary>
public sealed record NoteResponse(
    int NotaId,
    string? Descripcion,
    string UsuarioCreacion,
    DateTime FechaCreacion,
    int DocId
);

