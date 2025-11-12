using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Contracts.Preload.Dashboard;
using GeCom.Following.Preload.SharedKernel.Results;
using Microsoft.Extensions.Caching.Memory;

namespace GeCom.Following.Preload.Application.Features.Preload.Dashboard.GetDashboard;

/// <summary>
/// Handler for the GetDashboardQuery.
/// </summary>
internal sealed class GetDashboardQueryHandler : IQueryHandler<GetDashboardQuery, DashboardResponse>
{
    private const string CacheKey = "Dashboard_Statistics";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(2);

    private readonly IDocumentRepository _documentRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDashboardQueryHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="purchaseOrderRepository">The purchase order repository.</param>
    /// <param name="memoryCache">The memory cache.</param>
    public GetDashboardQueryHandler(
        IDocumentRepository documentRepository,
        IPurchaseOrderRepository purchaseOrderRepository,
        IMemoryCache memoryCache)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _purchaseOrderRepository = purchaseOrderRepository ?? throw new ArgumentNullException(nameof(purchaseOrderRepository));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    /// <inheritdoc />
    public async Task<Result<DashboardResponse>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Try to get from cache first
        if (_memoryCache.TryGetValue<DashboardResponse>(CacheKey, out DashboardResponse? cachedResponse) && cachedResponse is not null)
        {
            return Result.Success(cachedResponse);
        }

        // Execute COUNT queries sequentially to avoid DbContext concurrency issues
        // Note: DbContext is not thread-safe, so parallel queries on the same context would cause errors
        int totalDocuments =
            await _documentRepository.CountAsync(predicate: null, cancellationToken);

        int totalPurchaseOrders =
            await _purchaseOrderRepository.CountAsync(predicate: null, cancellationToken);

        int totalPendingsDocuments =
            await _documentRepository.CountAsync(
                predicate: d => d.EstadoId == 2 || d.EstadoId == 5,
                cancellationToken);

        DashboardResponse response = new(
            totalDocuments,
            totalPurchaseOrders,
            totalPendingsDocuments);

        // Cache the result
        MemoryCacheEntryOptions cacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = CacheExpiration,
            SlidingExpiration = null,
            Priority = CacheItemPriority.Normal
        };

        _memoryCache.Set(CacheKey, response, cacheOptions);

        return Result.Success(response);
    }
}

