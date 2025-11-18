using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.GetAllUserSocietyAssignmentsPaged;

/// <summary>
/// Handler for the GetAllUserSocietyAssignmentsPagedQuery.
/// </summary>
internal sealed class GetAllUserSocietyAssignmentsPagedQueryHandler : IQueryHandler<GetAllUserSocietyAssignmentsPagedQuery, PagedResponse<UserSocietyAssignmentResponse>>
{
    private readonly IUserSocietyAssignmentRepository _userSocietyAssignmentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllUserSocietyAssignmentsPagedQueryHandler"/> class.
    /// </summary>
    /// <param name="userSocietyAssignmentRepository">The user society assignment repository.</param>
    public GetAllUserSocietyAssignmentsPagedQueryHandler(IUserSocietyAssignmentRepository userSocietyAssignmentRepository)
    {
        _userSocietyAssignmentRepository = userSocietyAssignmentRepository ?? throw new ArgumentNullException(nameof(userSocietyAssignmentRepository));
    }

    /// <inheritdoc />
    public async Task<Result<PagedResponse<UserSocietyAssignmentResponse>>> Handle(GetAllUserSocietyAssignmentsPagedQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        (IReadOnlyList<Domain.Preloads.UserSocietyAssignments.UserSocietyAssignment> Items, int TotalCount) page =
            await _userSocietyAssignmentRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        IReadOnlyList<UserSocietyAssignmentResponse> mapped = page.Items
            .Select(UserSocietyAssignmentMappings.ToResponse)
            .ToList();

        PagedResponse<UserSocietyAssignmentResponse> response = new(mapped, page.TotalCount, request.Page, request.PageSize);

        return Result.Success(response);
    }
}

