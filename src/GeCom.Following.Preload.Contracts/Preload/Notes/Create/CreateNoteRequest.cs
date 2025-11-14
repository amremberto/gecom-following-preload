namespace GeCom.Following.Preload.Contracts.Preload.Notes.Create;

/// <summary>
/// Request DTO for creating a new note.
/// </summary>
public sealed record CreateNoteRequest(
    int DocId,
    string Descripcion,
    string UsuarioCreacion
);

