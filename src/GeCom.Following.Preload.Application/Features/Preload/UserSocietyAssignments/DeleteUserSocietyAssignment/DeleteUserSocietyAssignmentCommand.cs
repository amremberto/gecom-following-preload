using GeCom.Following.Preload.Application.Abstractions.Messaging;

namespace GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.DeleteUserSocietyAssignment;

/// <summary>
/// Command to delete a user society assignment by its ID.
/// </summary>
public sealed record DeleteUserSocietyAssignmentCommand(int Id) : ICommand;

