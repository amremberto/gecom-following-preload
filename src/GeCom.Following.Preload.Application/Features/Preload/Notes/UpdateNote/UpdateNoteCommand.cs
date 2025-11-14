using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Notes;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.UpdateNote;

/// <summary>
/// Command to update an existing note.
/// </summary>
public sealed record UpdateNoteCommand(
    int NotaId,
    string Descripcion) : ICommand<NoteResponse>;

