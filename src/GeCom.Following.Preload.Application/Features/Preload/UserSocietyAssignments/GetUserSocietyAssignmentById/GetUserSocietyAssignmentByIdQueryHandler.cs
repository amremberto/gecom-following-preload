using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.UserSocietyAssignments;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.UserSocietyAssignments.GetUserSocietyAssignmentById;

/// <summary>
/// Handler for the GetUserSocietyAssignmentByIdQuery.
/// </summary>
internal sealed class GetUserSocietyAssignmentByIdQueryHandler : IQueryHandler<GetUserSocietyAssignmentByIdQuery, UserSocietyAssignmentResponse>
{
    private readonly IUserSocietyAssignmentRepository _userSocietyAssignmentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserSocietyAssignmentByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="userSocietyAssignmentRepository">The user society assignment repository.</param>
    public GetUserSocietyAssignmentByIdQueryHandler(IUserSocietyAssignmentRepository userSocietyAssignmentRepository)
    {
        _userSocietyAssignmentRepository = userSocietyAssignmentRepository ?? throw new ArgumentNullException(nameof(userSocietyAssignmentRepository));
    }

    /// <inheritdoc />
    public async Task<Result<UserSocietyAssignmentResponse>> Handle(GetUserSocietyAssignmentByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Domain.Preloads.UserSocietyAssignments.UserSocietyAssignment? assignment = await _userSocietyAssignmentRepository.GetByIdAsync(request.Id, cancellationToken);

        if (assignment is null)
        {
            return Result.Failure<UserSocietyAssignmentResponse>(
                Error.NotFound(
                    "UserSocietyAssignment.NotFound",
                    $"UserSocietyAssignment with ID '{request.Id}' was not found."));
        }

        UserSocietyAssignmentResponse response = UserSocietyAssignmentMappings.ToResponse(assignment);

        return Result.Success(response);
    }
}

