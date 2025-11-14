using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Notes;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.GetAllNotes;

/// <summary>
/// Handler for the GetAllNotesQuery.
/// </summary>
internal sealed class GetAllNotesQueryHandler : IQueryHandler<GetAllNotesQuery, IEnumerable<NoteResponse>>
{
    private readonly INoteRepository _noteRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllNotesQueryHandler"/> class.
    /// </summary>
    /// <param name="noteRepository">The note repository.</param>
    public GetAllNotesQueryHandler(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<NoteResponse>>> Handle(GetAllNotesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<Domain.Preloads.Notes.Note> notes = await _noteRepository.GetAllAsync(cancellationToken);

        IEnumerable<NoteResponse> response = notes
            .OrderByDescending(n => n.FechaCreacion)
            .Select(NoteMappings.ToResponse);

        return Result.Success(response);
    }
}

