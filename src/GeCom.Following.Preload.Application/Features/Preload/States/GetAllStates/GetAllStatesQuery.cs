using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.States;

namespace GeCom.Following.Preload.Application.Features.Preload.States.GetAllStates;

/// <summary>
/// Query to get all states.
/// </summary>
public sealed record GetAllStatesQuery() : IQuery<IEnumerable<StateResponse>>;

