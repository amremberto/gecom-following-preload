using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.Societies;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.DeleteSociety;

/// <summary>
/// Handler for the DeleteSocietyCommand.
/// </summary>
internal sealed class DeleteSocietyCommandHandler : ICommandHandler<DeleteSocietyCommand>
{
    private readonly ISocietyRepository _societyRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteSocietyCommandHandler"/> class.
    /// </summary>
    /// <param name="societyRepository">The society repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public DeleteSocietyCommandHandler(ISocietyRepository societyRepository, IUnitOfWork unitOfWork)
    {
        _societyRepository = societyRepository ?? throw new ArgumentNullException(nameof(societyRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteSocietyCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si la Society existe
        Society? society = await _societyRepository.GetByIdAsync(request.Id, cancellationToken);
        if (society is null)
        {
            return Result.Failure(
                Error.NotFound(
                    "Society.NotFound",
                    $"Society with ID '{request.Id}' was not found."));
        }

        // Eliminar la Society
        await _societyRepository.RemoveByIdAsync(request.Id, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

