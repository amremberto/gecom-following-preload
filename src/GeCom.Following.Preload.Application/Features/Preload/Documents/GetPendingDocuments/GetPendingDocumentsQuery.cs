using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Documents;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetPendingDocuments;

/// <summary>
/// Query to get pending documents based on user role.
/// Pending documents are those with EstadoId == 1, EstadoId == 2 or EstadoId == 5 and have FechaEmisionComprobante set.
/// </summary>
/// <param name="UserRoles">User roles to determine filtering strategy. Must contain at least one role.</param>
/// <param name="UserEmail">User email (required).</param>
public sealed record GetPendingDocumentsQuery(
    IReadOnlyList<string> UserRoles,
    string UserEmail
) : IQuery<IEnumerable<DocumentResponse>>;
