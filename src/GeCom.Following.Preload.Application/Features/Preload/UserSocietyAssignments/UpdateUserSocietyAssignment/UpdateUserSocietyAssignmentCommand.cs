using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments;

namespace GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.UpdateUserSocietyAssignment;

/// <summary>
/// Command to update an existing user society assignment.
/// </summary>
public sealed record UpdateUserSocietyAssignmentCommand(
    int Id,
    string Email,
    string CuitClient,
    string SociedadFi) : ICommand<UserSocietyAssignmentResponse>;

