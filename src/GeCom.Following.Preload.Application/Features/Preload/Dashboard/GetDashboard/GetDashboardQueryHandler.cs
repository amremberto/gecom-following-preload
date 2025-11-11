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
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDashboardQueryHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="purchaseOrderRepository">The purchase order repository.</param>
    public GetDashboardQueryHandler(IDocumentRepository documentRepository, IPurchaseOrderRepository purchaseOrderRepository)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _purchaseOrderRepository = purchaseOrderRepository ?? throw new ArgumentNullException(nameof(purchaseOrderRepository));
    }

    /// <inheritdoc />
    public async Task<Result<DashboardResponse>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        int totalDocuments =
            await _documentRepository.CountAsync(predicate: null, cancellationToken);

        int totalPurchaseOrders =
            await _purchaseOrderRepository.CountAsync(predicate: null, cancellationToken);

        int totalPendingsDocuments =
            await _documentRepository.CountAsync(
                predicate: d => d.EstadoId == 2 || d.EstadoId == 5,
                cancellationToken);

        DashboardResponse response = new(totalDocuments, totalPurchaseOrders, totalPendingsDocuments);

        return Result.Success(response);
    }
}

