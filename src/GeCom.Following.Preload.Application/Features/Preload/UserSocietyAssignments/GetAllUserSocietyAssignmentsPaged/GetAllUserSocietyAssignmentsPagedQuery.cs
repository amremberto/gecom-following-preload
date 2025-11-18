using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.GetAllUserSocietyAssignmentsPaged;

/// <summary>
/// Query to get user society assignments with pagination.
/// </summary>
public sealed record GetAllUserSocietyAssignmentsPagedQuery(int Page, int PageSize) : IQuery<PagedResponse<UserSocietyAssignmentResponse>>;

