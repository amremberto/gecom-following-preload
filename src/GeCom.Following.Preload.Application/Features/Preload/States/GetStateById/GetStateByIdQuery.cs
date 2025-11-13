using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.States;

namespace GeCom.Following.Preload.Application.Features.Preload.States.GetStateById;

/// <summary>
/// Query to get a state by its ID.
/// </summary>
public sealed record GetStateByIdQuery(int EstadoId) : IQuery<StateResponse>;

