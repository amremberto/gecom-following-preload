using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.Domain.Preloads.Societies;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietiesByUserEmail;

/// <summary>
/// Handler for the GetSocietiesByUserEmailQuery.
/// </summary>
internal sealed class GetSocietiesByUserEmailQueryHandler : IQueryHandler<GetSocietiesByUserEmailQuery, IEnumerable<SocietySelectItem>>
{
    private readonly IUserSocietyAssignmentRepository _userSocietyAssignmentRepository;
    private readonly ISocietyRepository _societyRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSocietiesByUserEmailQueryHandler"/> class.
    /// </summary>
    /// <param name="userSocietyAssignmentRepository">The user society assignment repository.</param>
    /// <param name="societyRepository">The society repository.</param>
    public GetSocietiesByUserEmailQueryHandler(
        IUserSocietyAssignmentRepository userSocietyAssignmentRepository,
        ISocietyRepository societyRepository)
    {
        _userSocietyAssignmentRepository = userSocietyAssignmentRepository ?? throw new ArgumentNullException(nameof(userSocietyAssignmentRepository));
        _societyRepository = societyRepository ?? throw new ArgumentNullException(nameof(societyRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<SocietySelectItem>>> Handle(GetSocietiesByUserEmailQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserEmail);

        // Get all society assignments for the user
        IEnumerable<Domain.Preloads.UserSocietyAssignments.UserSocietyAssignment> assignments =
            await _userSocietyAssignmentRepository.GetByEmailAsync(request.UserEmail, cancellationToken);

        var societyCuits = assignments
            .Select(a => a.CuitClient)
            .Distinct()
            .ToList();

        if (societyCuits.Count == 0)
        {
            // User has no society assignments, return empty result
            return Result.Success(Enumerable.Empty<SocietySelectItem>());
        }

        // Get societies by CUITs
        IEnumerable<Society> societies =
            await _societyRepository.GetByCuitsAsync(societyCuits, cancellationToken);

        // Map to select item DTO (only CUIT and Descripción)
        IEnumerable<SocietySelectItem> response = societies
            .Select(s => new SocietySelectItem(s.Cuit, s.Descripcion))
            .OrderBy(s => s.Descripcion)
            .ToList();

        return Result.Success(response);
    }
}

