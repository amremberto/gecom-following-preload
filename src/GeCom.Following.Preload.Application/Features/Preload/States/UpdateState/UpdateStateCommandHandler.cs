using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.States;
using GeCom.Following.Preload.Domain.Preloads.States;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.States.UpdateState;

/// <summary>
/// Handler for the UpdateStateCommand.
/// </summary>
internal sealed class UpdateStateCommandHandler : ICommandHandler<UpdateStateCommand, StateResponse>
{
    private readonly IStateRepository _stateRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateStateCommandHandler"/> class.
    /// </summary>
    /// <param name="stateRepository">The state repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public UpdateStateCommandHandler(IStateRepository stateRepository, IUnitOfWork unitOfWork)
    {
        _stateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<StateResponse>> Handle(UpdateStateCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si el State existe
        State? state = await _stateRepository.GetByEstadoIdAsync(request.EstadoId, cancellationToken);
        if (state is null)
        {
            return Result.Failure<StateResponse>(
                Error.NotFound(
                    "State.NotFound",
                    $"State with ID '{request.EstadoId}' was not found."));
        }

        // Verificar si ya existe otro State con el mismo código (excluyendo el actual)
        State? existingByCode = await _stateRepository.GetByCodeAsync(request.Codigo, cancellationToken);
        if (existingByCode is not null && existingByCode.EstadoId != request.EstadoId)
        {
            return Result.Failure<StateResponse>(
                Error.Conflict(
                    "State.Conflict",
                    $"A state with code '{request.Codigo}' already exists."));
        }

        // Actualizar los campos
        state.Descripcion = request.Descripcion;
        state.Codigo = request.Codigo;

        // Actualizar en el repositorio
        State updatedState = await _stateRepository.UpdateAsync(state, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        StateResponse response = StateMappings.ToResponse(updatedState);

        return Result.Success(response);
    }
}

