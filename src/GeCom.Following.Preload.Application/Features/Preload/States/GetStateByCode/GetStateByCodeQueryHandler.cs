using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.States;
using GeCom.Following.Preload.Domain.Preloads.States;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.States.GetStateByCode;

/// <summary>
/// Handler for the GetStateByCodeQuery.
/// </summary>
internal sealed class GetStateByCodeQueryHandler : IQueryHandler<GetStateByCodeQuery, StateResponse>
{
    private readonly IStateRepository _stateRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetStateByCodeQueryHandler"/> class.
    /// </summary>
    /// <param name="stateRepository">The state repository.</param>
    public GetStateByCodeQueryHandler(IStateRepository stateRepository)
    {
        _stateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
    }

    /// <inheritdoc />
    public async Task<Result<StateResponse>> Handle(GetStateByCodeQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        State? state = await _stateRepository.GetByCodeAsync(request.Codigo, cancellationToken);

        if (state is null)
        {
            return Result.Failure<StateResponse>(
                Error.NotFound(
                    "State.NotFound",
                    $"State with code '{request.Codigo}' was not found."));
        }

        StateResponse response = StateMappings.ToResponse(state);

        return Result.Success(response);
    }
}

