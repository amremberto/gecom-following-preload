using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.Notes;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.DeleteNote;

/// <summary>
/// Handler for the DeleteNoteCommand.
/// </summary>
internal sealed class DeleteNoteCommandHandler : ICommandHandler<DeleteNoteCommand>
{
    private readonly INoteRepository _noteRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteNoteCommandHandler"/> class.
    /// </summary>
    /// <param name="noteRepository">The note repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public DeleteNoteCommandHandler(
        INoteRepository noteRepository,
        IUnitOfWork unitOfWork)
    {
        _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si la nota existe
        Note? note = await _noteRepository.GetByNotaIdAsync(request.NotaId, cancellationToken);
        if (note is null)
        {
            return Result.Failure(
                Error.NotFound(
                    "Note.NotFound",
                    $"Note with ID '{request.NotaId}' was not found."));
        }

        // Eliminar la nota
        await _noteRepository.RemoveByIdAsync(request.NotaId, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

