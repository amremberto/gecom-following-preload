using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Notes;
using GeCom.Following.Preload.Domain.Preloads.Notes;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.GetNoteById;

/// <summary>
/// Handler for the GetNoteByIdQuery.
/// </summary>
internal sealed class GetNoteByIdQueryHandler : IQueryHandler<GetNoteByIdQuery, NoteResponse>
{
    private readonly INoteRepository _noteRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetNoteByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="noteRepository">The note repository.</param>
    public GetNoteByIdQueryHandler(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
    }

    /// <inheritdoc />
    public async Task<Result<NoteResponse>> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Note? note = await _noteRepository.GetByNotaIdAsync(request.NotaId, cancellationToken);

        if (note is null)
        {
            return Result.Failure<NoteResponse>(
                Error.NotFound(
                    "Note.NotFound",
                    $"Note with ID '{request.NotaId}' was not found."));
        }

        NoteResponse response = NoteMappings.ToResponse(note);

        return Result.Success(response);
    }
}

