using GeCom.Following.Preload.Application.Abstractions.Messaging;

namespace GeCom.Following.Preload.Application.Features.Preload.States.DeleteState;

/// <summary>
/// Command to delete a state by its ID.
/// </summary>
public sealed record DeleteStateCommand(int EstadoId) : ICommand;

