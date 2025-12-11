using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.Domain.Preloads.UserSocietyAssignments;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetPendingDocuments;

/// <summary>
/// Handler for the GetPendingDocumentsQuery.
/// Determines filtering strategy based on user role:
/// - Administrator/ReadOnly: Returns all pending documents
/// - Societies: Returns pending documents for all societies assigned to the user
/// </summary>
internal sealed class GetPendingDocumentsQueryHandler
    : IQueryHandler<GetPendingDocumentsQuery, IEnumerable<DocumentResponse>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserSocietyAssignmentRepository _userSocietyAssignmentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPendingDocumentsQueryHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="userSocietyAssignmentRepository">The user society assignment repository.</param>
    public GetPendingDocumentsQueryHandler(
        IDocumentRepository documentRepository,
        IUserSocietyAssignmentRepository userSocietyAssignmentRepository)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _userSocietyAssignmentRepository = userSocietyAssignmentRepository ?? throw new ArgumentNullException(nameof(userSocietyAssignmentRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<DocumentResponse>>> Handle(
        GetPendingDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<Document> documents;

        // Determine filtering strategy based on user roles
        // Using role constants directly to avoid dependency on WebApi layer
        const string followingAdministrator = "Following.Administrator";
        const string followingPreloadReadOnly = "Following.Preload.ReadOnly";
        const string followingPreloadSocieties = "Following.Preload.Societies";

        if (HasRole(request.UserRoles, followingAdministrator) ||
            HasRole(request.UserRoles, followingPreloadReadOnly))
        {
            // Administrator or ReadOnly: Return all pending documents
            documents = await _documentRepository.GetPendingAsync(cancellationToken);
        }
        else if (HasRole(request.UserRoles, followingPreloadSocieties))
        {
            // Societies: Filter by all societies assigned to the user
            // UserEmail is validated by FluentValidation, so it's guaranteed to be present and valid

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
                documents = await _documentRepository.GetPendingBySocietyCuitsAsync(
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

