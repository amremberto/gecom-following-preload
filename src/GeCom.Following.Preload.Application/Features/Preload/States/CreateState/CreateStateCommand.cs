using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.States;

namespace GeCom.Following.Preload.Application.Features.Preload.States.CreateState;

/// <summary>
/// Command to create a new state.
/// </summary>
public sealed record CreateStateCommand(
    string Descripcion,
    string Codigo) : ICommand<StateResponse>;

