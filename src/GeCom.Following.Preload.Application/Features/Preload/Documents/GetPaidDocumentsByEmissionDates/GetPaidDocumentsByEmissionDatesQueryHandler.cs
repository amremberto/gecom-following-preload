using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.Domain.Preloads.States;
using GeCom.Following.Preload.Domain.Preloads.UserSocietyAssignments;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetPaidDocumentsByEmissionDates;

/// <summary>
/// Handler for the GetPaidDocumentsByEmissionDatesQuery.
/// Determines filtering strategy based on user role:
/// - Providers: Filters by provider CUIT from claim
/// - Societies: Filters by all societies assigned to the user
/// - Administrator/ReadOnly: Returns all paid documents without filtering
/// Paid documents are those with state code "PagadoFin".
/// </summary>
internal sealed class GetPaidDocumentsByEmissionDatesQueryHandler
    : IQueryHandler<GetPaidDocumentsByEmissionDatesQuery, IEnumerable<DocumentResponse>>
{
    private const string PaidStateCode = "PagadoFin";
    private readonly IDocumentRepository _documentRepository;
    private readonly IStateRepository _stateRepository;
    private readonly IUserSocietyAssignmentRepository _userSocietyAssignmentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPaidDocumentsByEmissionDatesQueryHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="stateRepository">The state repository.</param>
    /// <param name="userSocietyAssignmentRepository">The user society assignment repository.</param>
    public GetPaidDocumentsByEmissionDatesQueryHandler(
        IDocumentRepository documentRepository,
        IStateRepository stateRepository,
        IUserSocietyAssignmentRepository userSocietyAssignmentRepository)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _stateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
        _userSocietyAssignmentRepository = userSocietyAssignmentRepository ?? throw new ArgumentNullException(nameof(userSocietyAssignmentRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<DocumentResponse>>> Handle(
        GetPaidDocumentsByEmissionDatesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Get the EstadoId for "PagadoFin" state code
        State? paidState = await _stateRepository.GetByCodeAsync(PaidStateCode, cancellationToken);

        if (paidState is null)
        {
            return Result.Failure<IEnumerable<DocumentResponse>>(
                Error.NotFound(
                    "State.NotFound",
                    $"State with code '{PaidStateCode}' was not found."));
        }

        int paidEstadoId = paidState.EstadoId;

        IEnumerable<Document> documents;

        // Determine filtering strategy based on user roles
        // Using role constants directly to avoid dependency on WebApi layer
        const string followingAdministrator = "Following.Administrator";
        const string followingPreloadReadOnly = "Following.Preload.ReadOnly";
        const string followingPreloadProviders = "Following.Preload.Providers";
        const string followingPreloadSocieties = "Following.Preload.Societies";

        if (HasRole(request.UserRoles, followingAdministrator) ||
            HasRole(request.UserRoles, followingPreloadReadOnly))
        {
            // Administrator or ReadOnly: Return all paid documents
            documents = await _documentRepository.GetByEmissionDatesAndEstadoIdAsync(
                request.DateFrom,
                request.DateTo,
                paidEstadoId,
                providerCuit: null,
                cancellationToken);
        }
        else if (HasRole(request.UserRoles, followingPreloadProviders))
        {
            // Providers: Filter by provider CUIT from claim
            if (string.IsNullOrWhiteSpace(request.ProviderCuit))
            {
                return Result.Failure<IEnumerable<DocumentResponse>>(
                    Error.Failure(
                        "Document.ProviderCuitRequired",
                        "Provider CUIT is required for users with Providers role."));
            }

            documents = await _documentRepository.GetByEmissionDatesAndEstadoIdAsync(
                request.DateFrom,
                request.DateTo,
                paidEstadoId,
                request.ProviderCuit,
                cancellationToken);
        }
        else if (HasRole(request.UserRoles, followingPreloadSocieties))
        {
            // Societies: Filter by all societies assigned to the user
            if (string.IsNullOrWhiteSpace(request.UserEmail))
            {
                return Result.Failure<IEnumerable<DocumentResponse>>(
                    Error.Failure(
                        "Document.UserEmailRequired",
                        "User email is required for users with Societies role."));
            }

            // Get all society assignments for the user
            IEnumerable<UserSocietyAssignment> assignments =
                await _userSocietyAssignmentRepository.GetByEmailAsync(request.UserEmail, cancellationToken);

            var societyCuits = assignments
                .Select(a => a.CuitClient)
                .Distinct()
                .ToList();

            if (societyCuits.Count == 0)
            {
                // User has no society assignments, return empty result
                documents = Enumerable.Empty<Document>();
            }
            else
            {
                documents = await _documentRepository.GetByEmissionDatesEstadoIdAndSocietyCuitsAsync(
                    request.DateFrom,
                    request.DateTo,
                    paidEstadoId,
                    societyCuits,
                    cancellationToken);
            }
        }
        else
        {
            // Unknown role: Return empty result for security
            documents = Enumerable.Empty<Document>();
        }

        IEnumerable<DocumentResponse> response = documents.Select(DocumentMappings.ToResponse);

        return Result.Success(response);
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
