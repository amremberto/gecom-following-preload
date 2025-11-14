using GeCom.Following.Preload.Application.Abstractions.Messaging;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.DeleteNote;

/// <summary>
/// Command to delete a note by its ID.
/// </summary>
public sealed record DeleteNoteCommand(int NotaId) : ICommand;

