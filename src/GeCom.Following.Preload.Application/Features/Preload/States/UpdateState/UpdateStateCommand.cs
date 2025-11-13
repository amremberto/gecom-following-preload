using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.States;

namespace GeCom.Following.Preload.Application.Features.Preload.States.UpdateState;

/// <summary>
/// Command to update an existing state.
/// </summary>
public sealed record UpdateStateCommand(
    int EstadoId,
    string Descripcion,
    string Codigo) : ICommand<StateResponse>;

