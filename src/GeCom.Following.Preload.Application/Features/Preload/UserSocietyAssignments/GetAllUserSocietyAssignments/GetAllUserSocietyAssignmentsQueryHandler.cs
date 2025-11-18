using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.GetAllUserSocietyAssignments;

/// <summary>
/// Handler for the GetAllUserSocietyAssignmentsQuery.
/// </summary>
internal sealed class GetAllUserSocietyAssignmentsQueryHandler : IQueryHandler<GetAllUserSocietyAssignmentsQuery, IEnumerable<UserSocietyAssignmentResponse>>
{
    private readonly IUserSocietyAssignmentRepository _userSocietyAssignmentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllUserSocietyAssignmentsQueryHandler"/> class.
    /// </summary>
    /// <param name="userSocietyAssignmentRepository">The user society assignment repository.</param>
    public GetAllUserSocietyAssignmentsQueryHandler(IUserSocietyAssignmentRepository userSocietyAssignmentRepository)
    {
        _userSocietyAssignmentRepository = userSocietyAssignmentRepository ?? throw new ArgumentNullException(nameof(userSocietyAssignmentRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<UserSocietyAssignmentResponse>>> Handle(GetAllUserSocietyAssignmentsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<Domain.Preloads.UserSocietyAssignments.UserSocietyAssignment> assignments = await _userSocietyAssignmentRepository.GetAllAsync(cancellationToken);

        IEnumerable<UserSocietyAssignmentResponse> response = assignments.Select(UserSocietyAssignmentMappings.ToResponse);

        return Result.Success(response);
    }
}

