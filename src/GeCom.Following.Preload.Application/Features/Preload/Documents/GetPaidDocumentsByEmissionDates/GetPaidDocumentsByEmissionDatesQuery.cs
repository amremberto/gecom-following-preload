using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Documents;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetPaidDocumentsByEmissionDates;

/// <summary>
/// Query to get paid documents by emission date range based on user role.
/// Paid documents are those with state code "PagadoFin".
/// </summary>
/// <param name="DateFrom">Start emission date.</param>
/// <param name="DateTo">End emission date.</param>
/// <param name="UserRoles">User roles to determine filtering strategy.</param>
/// <param name="UserEmail">User email (required for Societies role).</param>
/// <param name="ProviderCuit">Provider CUIT from claim (required for Providers role).</param>
public sealed record GetPaidDocumentsByEmissionDatesQuery(
    DateOnly DateFrom,
    DateOnly DateTo,
    IReadOnlyList<string> UserRoles,
    string? UserEmail = null,
    string? ProviderCuit = null
) : IQuery<IEnumerable<DocumentResponse>>;
