using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.States;

namespace GeCom.Following.Preload.Application.Features.Preload.States.GetStateByCode;

/// <summary>
/// Query to get a state by its code.
/// </summary>
public sealed record GetStateByCodeQuery(string Codigo) : IQuery<StateResponse>;

