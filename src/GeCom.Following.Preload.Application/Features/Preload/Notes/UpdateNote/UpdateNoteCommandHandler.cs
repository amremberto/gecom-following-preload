using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Notes;
using GeCom.Following.Preload.Domain.Preloads.Notes;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.UpdateNote;

/// <summary>
/// Handler for the UpdateNoteCommand.
/// </summary>
internal sealed class UpdateNoteCommandHandler : ICommandHandler<UpdateNoteCommand, NoteResponse>
{
    private readonly INoteRepository _noteRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateNoteCommandHandler"/> class.
    /// </summary>
    /// <param name="noteRepository">The note repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public UpdateNoteCommandHandler(
        INoteRepository noteRepository,
        IUnitOfWork unitOfWork)
    {
        _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<NoteResponse>> Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si la nota existe
        Note? note = await _noteRepository.GetByNotaIdAsync(request.NotaId, cancellationToken);
        if (note is null)
        {
            return Result.Failure<NoteResponse>(
                Error.NotFound(
                    "Note.NotFound",
                    $"Note with ID '{request.NotaId}' was not found."));
        }

        // Actualizar los campos
        note.Descripcion = request.Descripcion;

        // Actualizar en el repositorio
        Note updatedNote = await _noteRepository.UpdateAsync(note, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        NoteResponse response = NoteMappings.ToResponse(updatedNote);

        return Result.Success(response);
    }
}

