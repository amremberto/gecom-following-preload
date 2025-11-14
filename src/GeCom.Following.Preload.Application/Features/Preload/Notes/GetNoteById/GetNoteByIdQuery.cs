using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Notes;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.GetNoteById;

/// <summary>
/// Query to get a note by its ID.
/// </summary>
public sealed record GetNoteByIdQuery(int NotaId) : IQuery<NoteResponse>;

