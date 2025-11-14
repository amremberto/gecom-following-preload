using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Notes;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.GetAllNotes;

/// <summary>
/// Query to get all notes.
/// </summary>
public sealed record GetAllNotesQuery : IQuery<IEnumerable<NoteResponse>>;

