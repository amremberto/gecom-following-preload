using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.States;
using GeCom.Following.Preload.Domain.Preloads.States;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.States.CreateState;

/// <summary>
/// Handler for the CreateStateCommand.
/// </summary>
internal sealed class CreateStateCommandHandler : ICommandHandler<CreateStateCommand, StateResponse>
{
    private readonly IStateRepository _stateRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateStateCommandHandler"/> class.
    /// </summary>
    /// <param name="stateRepository">The state repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public CreateStateCommandHandler(IStateRepository stateRepository, IUnitOfWork unitOfWork)
    {
        _stateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<StateResponse>> Handle(CreateStateCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si ya existe un State con el mismo código
        State? existingByCode = await _stateRepository.GetByCodeAsync(request.Codigo, cancellationToken);
        if (existingByCode is not null)
        {
            return Result.Failure<StateResponse>(
                Error.Conflict(
                    "State.Conflict",
                    $"A state with code '{request.Codigo}' already exists."));
        }

        // Crear la nueva entidad State
        State state = new()
        {
            Descripcion = request.Descripcion,
            Codigo = request.Codigo,
            FechaCreacion = DateTime.UtcNow
        };

        // Agregar al repositorio
        State addedState = await _stateRepository.AddAsync(state, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        StateResponse response = StateMappings.ToResponse(addedState);

        return Result.Success(response);
    }
}

