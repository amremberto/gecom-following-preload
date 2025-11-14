using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Notes;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.GetNotesByDocumentId;

/// <summary>
/// Query to get all notes for a specific document.
/// </summary>
public sealed record GetNotesByDocumentIdQuery(int DocId) : IQuery<IEnumerable<NoteResponse>>;

