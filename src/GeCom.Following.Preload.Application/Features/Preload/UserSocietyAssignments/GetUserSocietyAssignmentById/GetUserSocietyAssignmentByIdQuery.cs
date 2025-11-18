using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments;

namespace GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.GetUserSocietyAssignmentById;

/// <summary>
/// Query to get a user society assignment by its ID.
/// </summary>
public sealed record GetUserSocietyAssignmentByIdQuery(int Id) : IQuery<UserSocietyAssignmentResponse>;

