using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.Domain.Preloads.UserSocietyAssignments;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetDocumentsByEmissionDates;

/// <summary>
/// Handler for the GetDocumentsByEmissionDatesQuery.
/// Determines filtering strategy based on user role:
/// - Providers: Filters by provider CUIT from claim
/// - Societies: Filters by all societies assigned to the user
/// - Administrator/ReadOnly: Returns all documents without filtering
/// </summary>
internal sealed class GetDocumentsByEmissionDatesQueryHandler
    : IQueryHandler<GetDocumentsByEmissionDatesQuery, IEnumerable<DocumentResponse>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserSocietyAssignmentRepository _userSocietyAssignmentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDocumentsByEmissionDatesQueryHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="userSocietyAssignmentRepository">The user society assignment repository.</param>
    public GetDocumentsByEmissionDatesQueryHandler(
        IDocumentRepository documentRepository,
        IUserSocietyAssignmentRepository userSocietyAssignmentRepository)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _userSocietyAssignmentRepository = userSocietyAssignmentRepository ?? throw new ArgumentNullException(nameof(userSocietyAssignmentRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<DocumentResponse>>> Handle(
        GetDocumentsByEmissionDatesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<Domain.Preloads.Documents.Document> documents;

        // Determine filtering strategy based on user roles
        // Using role constants directly to avoid dependency on WebApi layer
        const string followingAdministrator = "Following.Administrator";
        const string followingPreloadReadOnly = "Following.Preload.ReadOnly";
        const string followingPreloadProviders = "Following.Preload.Providers";
        const string followingPreloadSocieties = "Following.Preload.Societies";

        if (HasRole(request.UserRoles, followingAdministrator) ||
            HasRole(request.UserRoles, followingPreloadReadOnly))
        {
            // Administrator or ReadOnly: Return all documents
            documents = await _documentRepository.GetByEmissionDatesAndProviderCuitAsync(
                request.DateFrom,
                request.DateTo,
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

            documents = await _documentRepository.GetByEmissionDatesAndProviderCuitAsync(
                request.DateFrom,
                request.DateTo,
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
                documents = await _documentRepository.GetByEmissionDatesAndSocietyCuitsAsync(
                    request.DateFrom,
                    request.DateTo,
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

