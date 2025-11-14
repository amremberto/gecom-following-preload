using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Notes;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.GetNotesByDocumentId;

/// <summary>
/// Handler for the GetNotesByDocumentIdQuery.
/// </summary>
internal sealed class GetNotesByDocumentIdQueryHandler : IQueryHandler<GetNotesByDocumentIdQuery, IEnumerable<NoteResponse>>
{
    private readonly INoteRepository _noteRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetNotesByDocumentIdQueryHandler"/> class.
    /// </summary>
    /// <param name="noteRepository">The note repository.</param>
    public GetNotesByDocumentIdQueryHandler(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<NoteResponse>>> Handle(GetNotesByDocumentIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<Domain.Preloads.Notes.Note> notes = await _noteRepository.GetByDocumentIdAsync(request.DocId, cancellationToken);

        IEnumerable<NoteResponse> response = notes
            .OrderByDescending(n => n.FechaCreacion)
            .Select(NoteMappings.ToResponse);

        return Result.Success(response);
    }
}

