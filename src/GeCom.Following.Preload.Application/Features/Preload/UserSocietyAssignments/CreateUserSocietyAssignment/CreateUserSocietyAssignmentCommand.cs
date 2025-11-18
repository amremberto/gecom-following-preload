using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments;

namespace GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.CreateUserSocietyAssignment;

/// <summary>
/// Command to create a new user society assignment.
/// </summary>
public sealed record CreateUserSocietyAssignmentCommand(
    string Email,
    string CuitClient,
    string SociedadFi) : ICommand<UserSocietyAssignmentResponse>;

