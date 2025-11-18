using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments;

namespace GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.GetAllUserSocietyAssignments;

/// <summary>
/// Query to get all user society assignments.
/// </summary>
public sealed record GetAllUserSocietyAssignmentsQuery() : IQuery<IEnumerable<UserSocietyAssignmentResponse>>;

