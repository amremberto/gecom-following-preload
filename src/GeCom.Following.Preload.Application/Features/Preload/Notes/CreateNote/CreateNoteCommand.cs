using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Notes;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.CreateNote;

/// <summary>
/// Command to create a new note.
/// </summary>
public sealed record CreateNoteCommand(
    int DocId,
    string Descripcion,
    string UsuarioCreacion) : ICommand<NoteResponse>;

