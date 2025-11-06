using GeCom.Following.Preload.Application.Abstractions.Messaging;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.DeleteSociety;

/// <summary>
/// Command to delete a society by its ID.
/// </summary>
public sealed record DeleteSocietyCommand(int Id) : ICommand;

