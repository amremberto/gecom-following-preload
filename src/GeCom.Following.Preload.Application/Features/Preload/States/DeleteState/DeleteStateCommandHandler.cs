using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.States;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.States.DeleteState;

/// <summary>
/// Handler for the DeleteStateCommand.
/// </summary>
internal sealed class DeleteStateCommandHandler : ICommandHandler<DeleteStateCommand>
{
    private readonly IStateRepository _stateRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteStateCommandHandler"/> class.
    /// </summary>
    /// <param name="stateRepository">The state repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public DeleteStateCommandHandler(IStateRepository stateRepository, IUnitOfWork unitOfWork)
    {
        _stateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteStateCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si el State existe
        State? state = await _stateRepository.GetByEstadoIdAsync(request.EstadoId, cancellationToken);
        if (state is null)
        {
            return Result.Failure(
                Error.NotFound(
                    "State.NotFound",
                    $"State with ID '{request.EstadoId}' was not found."));
        }

        // Eliminar el State
        await _stateRepository.RemoveByIdAsync(request.EstadoId, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

