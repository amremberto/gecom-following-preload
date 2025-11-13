using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.States;
using GeCom.Following.Preload.Domain.Preloads.States;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.States.GetAllStates;

/// <summary>
/// Handler for the GetAllStatesQuery.
/// </summary>
internal sealed class GetAllStatesQueryHandler : IQueryHandler<GetAllStatesQuery, IEnumerable<StateResponse>>
{
    private readonly IStateRepository _stateRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllStatesQueryHandler"/> class.
    /// </summary>
    /// <param name="stateRepository">The state repository.</param>
    public GetAllStatesQueryHandler(IStateRepository stateRepository)
    {
        _stateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<StateResponse>>> Handle(GetAllStatesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<State> states = await _stateRepository.GetAllAsync(cancellationToken);

        IEnumerable<StateResponse> response = states.Select(StateMappings.ToResponse);

        return Result.Success(response);
    }
}

