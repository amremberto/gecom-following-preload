using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Contracts.Preload.Dashboard;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Dashboard.GetDashboard;

/// <summary>
/// Handler for the GetDashboardQuery.
/// </summary>
internal sealed class GetDashboardQueryHandler : IQueryHandler<GetDashboardQuery, DashboardResponse>
{
    private readonly IDocumentRepository _documentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDashboardQueryHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    public GetDashboardQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    }

    /// <inheritdoc />
    public async Task<Result<DashboardResponse>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        int totalDocuments = await _documentRepository.CountAsync(predicate: null, cancellationToken);

        DashboardResponse response = new(totalDocuments);

        return Result.Success(response);
    }
}

