using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Features.Spd_Sap.SapProviderSocieties.GetProviderSocietiesByProviderCuit;
using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapProviderSocieties;
using GeCom.Following.Preload.Domain.Preloads.Societies;
using GeCom.Following.Preload.Domain.Preloads.UserSocietyAssignments;
using GeCom.Following.Preload.SharedKernel.Results;
using MediatR;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietiesForCurrentUser;

/// <summary>
/// Handler for the GetSocietiesForCurrentUserQuery.
/// Determines filtering strategy based on user role:
/// - Provider: Returns societies that the provider can assign documents to
/// - Society: Returns societies that the user has access to
/// - Administrator/ReadOnly: Returns empty result (societies are not needed for these roles in edit context)
/// </summary>
internal sealed class GetSocietiesForCurrentUserQueryHandler
    : IQueryHandler<GetSocietiesForCurrentUserQuery, IEnumerable<SocietySelectItemResponse>>
{
    private readonly IMediator _mediator;
    private readonly IUserSocietyAssignmentRepository _userSocietyAssignmentRepository;
    private readonly ISocietyRepository _societyRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSocietiesForCurrentUserQueryHandler"/> class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    /// <param name="userSocietyAssignmentRepository">The user society assignment repository.</param>
    /// <param name="societyRepository">The society repository.</param>
    public GetSocietiesForCurrentUserQueryHandler(
        IMediator mediator,
        IUserSocietyAssignmentRepository userSocietyAssignmentRepository,
        ISocietyRepository societyRepository)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _userSocietyAssignmentRepository = userSocietyAssignmentRepository ?? throw new ArgumentNullException(nameof(userSocietyAssignmentRepository));
        _societyRepository = societyRepository ?? throw new ArgumentNullException(nameof(societyRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<SocietySelectItemResponse>>> Handle(
        GetSocietiesForCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate that user roles are provided
        if (request.UserRoles.Count == 0)
        {
            return Result.Success(Enumerable.Empty<SocietySelectItemResponse>());
        }

        // Determine filtering strategy based on user roles
        // Priority: Provider role takes precedence over Society role if user has both
        // Using role constants directly to avoid dependency on WebApi layer
        const string followingPreloadProviders = "Following.Preload.Providers";
        const string followingPreloadSocieties = "Following.Preload.Societies";

        // Provider role: Get societies that the provider can assign documents to
        // This uses the SAP Provider-Society relationship table
        if (HasRole(request.UserRoles, followingPreloadProviders) &&
            !string.IsNullOrWhiteSpace(request.ProviderCuit))
        {
            GetProviderSocietiesByProviderCuitQuery query = new(request.ProviderCuit);
            Result<IEnumerable<ProviderSocietyResponse>> result = await _mediator.Send(query, cancellationToken);

            // Convert ProviderSocietyResponse to SocietySelectItemResponse
            // ProviderSocietyResponse.Name maps to SocietySelectItemResponse.Descripcion
            if (result.IsFailure)
            {
                return Result.Failure<IEnumerable<SocietySelectItemResponse>>(result.Error);
            }

            IEnumerable<SocietySelectItemResponse> response = result.Value
                .Select(ps => new SocietySelectItemResponse(
                    ps.Cuit,
                    ps.Name ?? string.Empty)) // Use Name from ProviderSocietyResponse as Descripcion
                .OrderBy(s => s.Descripcion)
                .ToList();

            return Result.Success(response);
        }

        // Society role: Get societies that the user has access to
        // This uses the UserSocietyAssignment table to find user's assigned societies
        if (HasRole(request.UserRoles, followingPreloadSocieties) &&
            !string.IsNullOrWhiteSpace(request.UserEmail))
        {
            // Get all society assignments for the user
            IEnumerable<UserSocietyAssignment> assignments =
                await _userSocietyAssignmentRepository.GetByEmailAsync(request.UserEmail, cancellationToken);

            var societyCuits = assignments
                .Select(a => a.CuitClient)
                .Where(cuit => !string.IsNullOrWhiteSpace(cuit)) // Filter out empty CUITs
                .Distinct()
                .ToList();

            if (societyCuits.Count == 0)
            {
                // User has no society assignments, return empty result
                return Result.Success(Enumerable.Empty<SocietySelectItemResponse>());
            }

            // Get societies by CUITs from the Society table
            IEnumerable<Society> societies =
                await _societyRepository.GetByCuitsAsync(societyCuits, cancellationToken);

            // Map to SocietySelectItemResponse (Cuit and Descripcion)
            IEnumerable<SocietySelectItemResponse> response = societies
                .Select(s => new SocietySelectItemResponse(s.Cuit, s.Descripcion))
                .OrderBy(s => s.Descripcion)
                .ToList();

            return Result.Success(response);
        }

        // Administrator/ReadOnly or unknown role: Return empty result
        // These roles typically don't need societies in the edit context
        return Result.Success(Enumerable.Empty<SocietySelectItemResponse>());
    }

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    /// <param name="userRoles">List of user roles.</param>
    /// <param name="role">Role to check.</param>
    /// <returns>True if the user has the role, false otherwise.</returns>
    private static bool HasRole(IReadOnlyList<string> userRoles, string role)
    {
        return userRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}

