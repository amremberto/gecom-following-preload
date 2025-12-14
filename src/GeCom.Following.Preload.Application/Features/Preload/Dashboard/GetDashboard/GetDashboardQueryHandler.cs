using System.Linq.Expressions;
using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Contracts.Preload.Dashboard;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.Domain.Preloads.PurchaseOrders;
using GeCom.Following.Preload.Domain.Preloads.UserSocietyAssignments;
using GeCom.Following.Preload.SharedKernel.Results;
using Microsoft.Extensions.Caching.Memory;

namespace GeCom.Following.Preload.Application.Features.Preload.Dashboard.GetDashboard;

/// <summary>
/// Handler for the GetDashboardQuery.
/// Determines filtering strategy based on user role:
/// - Providers: Filters by provider CUIT from claim
/// - Societies: Filters by all societies assigned to the user
/// - Administrator/ReadOnly: Returns all documents without filtering
/// </summary>
internal sealed class GetDashboardQueryHandler : IQueryHandler<GetDashboardQuery, DashboardResponse>
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(2);

    private readonly IDocumentRepository _documentRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUserSocietyAssignmentRepository _userSocietyAssignmentRepository;
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDashboardQueryHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="purchaseOrderRepository">The purchase order repository.</param>
    /// <param name="userSocietyAssignmentRepository">The user society assignment repository.</param>
    /// <param name="memoryCache">The memory cache.</param>
    public GetDashboardQueryHandler(
        IDocumentRepository documentRepository,
        IPurchaseOrderRepository purchaseOrderRepository,
        IUserSocietyAssignmentRepository userSocietyAssignmentRepository,
        IMemoryCache memoryCache)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _purchaseOrderRepository = purchaseOrderRepository ?? throw new ArgumentNullException(nameof(purchaseOrderRepository));
        _userSocietyAssignmentRepository = userSocietyAssignmentRepository ?? throw new ArgumentNullException(nameof(userSocietyAssignmentRepository));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    /// <inheritdoc />
    public async Task<Result<DashboardResponse>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Generate cache key based on user role and identifier
        string cacheKey = GenerateCacheKey(request);

        // Try to get from cache first
        if (_memoryCache.TryGetValue<DashboardResponse>(cacheKey, out DashboardResponse? cachedResponse) && cachedResponse is not null)
        {
            return Result.Success(cachedResponse);
        }

        // Determine filtering strategy based on user roles
        const string followingAdministrator = "Following.Administrator";
        const string followingPreloadReadOnly = "Following.Preload.ReadOnly";
        const string followingPreloadProviders = "Following.Preload.Providers";
        const string followingPreloadSocieties = "Following.Preload.Societies";

        string? providerCuit = null;
        List<string>? societyCuits = null;

        if (HasRole(request.UserRoles, followingAdministrator) ||
            HasRole(request.UserRoles, followingPreloadReadOnly))
        {
            // Administrator or ReadOnly: No filtering, return all documents
            providerCuit = null;
            societyCuits = null;
        }
        else if (HasRole(request.UserRoles, followingPreloadProviders))
        {
            // Providers: Filter by provider CUIT from claim
            if (string.IsNullOrWhiteSpace(request.ProviderCuit))
            {
                // If no CUIT, return zeros
                DashboardResponse emptyResponse = new(0, 0, 0, 0);
                return Result.Success(emptyResponse);
            }

            providerCuit = request.ProviderCuit;
        }
        else if (HasRole(request.UserRoles, followingPreloadSocieties))
        {
            // Societies: Filter by all societies assigned to the user
            if (string.IsNullOrWhiteSpace(request.UserEmail))
            {
                // If no email, return zeros
                DashboardResponse emptyResponse = new(0, 0, 0, 0);
                return Result.Success(emptyResponse);
            }

            // Get all society assignments for the user
            IEnumerable<UserSocietyAssignment> assignments =
                await _userSocietyAssignmentRepository.GetByEmailAsync(request.UserEmail, cancellationToken);

            societyCuits = assignments
                .Select(a => a.CuitClient)
                .Distinct()
                .ToList();

            if (societyCuits.Count == 0)
            {
                // User has no society assignments, return zeros
                DashboardResponse emptyResponse = new(0, 0, 0, 0);
                return Result.Success(emptyResponse);
            }
        }
        else
        {
            // Unknown role: Return zeros for security
            DashboardResponse emptyResponse = new(0, 0, 0, 0);
            return Result.Success(emptyResponse);
        }

        // Build predicates based on filtering strategy
        System.Linq.Expressions.Expression<System.Func<Document, bool>>? documentPredicate = null;
        System.Linq.Expressions.Expression<System.Func<PurchaseOrder, bool>>? purchaseOrderPredicate = null;

        if (providerCuit is not null)
        {
            // Filter by provider CUIT and ensure FechaEmisionComprobante is not null, FechaBaja is null, and EstadoId is not null
            // EstadoId != null ensures we only count documents with a defined state (pending or processed)
            string capturedProviderCuit = providerCuit;
            documentPredicate = d => d.FechaBaja == null && d.FechaEmisionComprobante.HasValue && d.EstadoId != null && d.ProveedorCuit == capturedProviderCuit;
            purchaseOrderPredicate = po => po.Document.FechaBaja == null && po.Document.FechaEmisionComprobante.HasValue && po.Document.EstadoId != null && po.Document.ProveedorCuit == capturedProviderCuit;
        }
        else if (societyCuits is not null && societyCuits.Count > 0)
        {
            // Filter by society CUITs
            // Build explicit OR conditions to avoid OPENJSON issues with SQL Server
            // EF Core uses OPENJSON for Contains() which fails on some SQL Server versions
            documentPredicate = BuildSocietyCuitDocumentPredicate(societyCuits);
            purchaseOrderPredicate = BuildSocietyCuitPurchaseOrderPredicate(societyCuits);
        }
        else
        {
            // Administrator or ReadOnly: Filter out deleted documents, require FechaEmisionComprobante, and EstadoId is not null
            // EstadoId != null ensures we only count documents with a defined state (pending or processed)
            // This ensures consistency with other roles and excludes soft-deleted documents
            documentPredicate = d => d.FechaBaja == null && d.FechaEmisionComprobante.HasValue && d.EstadoId != null;
            purchaseOrderPredicate = po => po.Document.FechaBaja == null && po.Document.FechaEmisionComprobante.HasValue && po.Document.EstadoId != null;
        }

        // Execute COUNT queries sequentially to avoid DbContext concurrency issues
        // Note: DbContext is not thread-safe, so parallel queries on the same context would cause errors
        int totalDocuments =
            await _documentRepository.CountAsync(predicate: documentPredicate, cancellationToken);

        int totalPurchaseOrders =
            await _purchaseOrderRepository.CountAsync(predicate: purchaseOrderPredicate, cancellationToken);

        // For pending documents, build predicate combining document filter with pending state
        System.Linq.Expressions.Expression<System.Func<Domain.Preloads.Documents.Document, bool>>? pendingPredicate = null;

        if (providerCuit is not null)
        {
            // Filter by provider CUIT AND pending state AND FechaEmisionComprobante is not null AND FechaBaja is null
            // Pending documents: EstadoId == 1, 2, or 5
            string capturedProviderCuit = providerCuit;
            pendingPredicate = d => d.FechaBaja == null && d.FechaEmisionComprobante.HasValue && d.ProveedorCuit == capturedProviderCuit && (d.EstadoId == 1 || d.EstadoId == 2 || d.EstadoId == 5);
        }
        else if (societyCuits is not null && societyCuits.Count > 0)
        {
            // Filter by society CUITs AND pending state
            // Build explicit OR conditions to avoid OPENJSON issues with SQL Server
            pendingPredicate = BuildSocietyCuitPendingDocumentPredicate(societyCuits);
        }
        else
        {
            // Only pending state filter AND FechaEmisionComprobante is not null AND FechaBaja is null
            // Pending documents: EstadoId == 1, 2, or 5
            pendingPredicate = d => d.FechaBaja == null && d.FechaEmisionComprobante.HasValue && (d.EstadoId == 1 || d.EstadoId == 2 || d.EstadoId == 5);
        }

        int totalPendingsDocuments =
            await _documentRepository.CountAsync(predicate: pendingPredicate, cancellationToken);

        // For paid documents, build predicate combining document filter with paid state (FechaPago != null AND EstadoId == 16)
        System.Linq.Expressions.Expression<System.Func<Domain.Preloads.Documents.Document, bool>>? paidPredicate = null;

        if (providerCuit is not null)
        {
            // Filter by provider CUIT AND paid state (FechaPago != null AND EstadoId == 16) AND FechaEmisionComprobante is not null AND FechaBaja is null
            string capturedProviderCuit = providerCuit;
            paidPredicate = d => d.FechaBaja == null && d.FechaEmisionComprobante.HasValue && d.FechaPago != null && d.EstadoId == 16 && d.ProveedorCuit == capturedProviderCuit;
        }
        else if (societyCuits is not null && societyCuits.Count > 0)
        {
            // Filter by society CUITs AND paid state
            // Build explicit OR conditions to avoid OPENJSON issues with SQL Server
            paidPredicate = BuildSocietyCuitPaidDocumentPredicate(societyCuits);
        }
        else
        {
            // Only paid state filter (FechaPago != null AND EstadoId == 16) AND FechaEmisionComprobante is not null AND FechaBaja is null
            paidPredicate = d => d.FechaBaja == null && d.FechaEmisionComprobante.HasValue && d.FechaPago != null && d.EstadoId == 16;
        }

        int totalPaidDocuments =
            await _documentRepository.CountAsync(predicate: paidPredicate, cancellationToken);

        DashboardResponse response = new(
            totalDocuments,
            totalPurchaseOrders,
            totalPendingsDocuments,
            totalPaidDocuments);

        // Cache the result
        MemoryCacheEntryOptions cacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = CacheExpiration,
            SlidingExpiration = null,
            Priority = CacheItemPriority.Normal
        };

        _memoryCache.Set(cacheKey, response, cacheOptions);

        return Result.Success(response);
    }

    /// <summary>
    /// Generates a cache key based on user role and identifier.
    /// </summary>
    /// <param name="request">The dashboard query request.</param>
    /// <returns>Cache key string.</returns>
    private static string GenerateCacheKey(GetDashboardQuery request)
    {
        const string followingAdministrator = "Following.Administrator";
        const string followingPreloadReadOnly = "Following.Preload.ReadOnly";
        const string followingPreloadProviders = "Following.Preload.Providers";
        const string followingPreloadSocieties = "Following.Preload.Societies";

        if (HasRole(request.UserRoles, followingAdministrator) ||
            HasRole(request.UserRoles, followingPreloadReadOnly))
        {
            return "Dashboard_Statistics_Admin";
        }
        else if (HasRole(request.UserRoles, followingPreloadProviders))
        {
            return $"Dashboard_Statistics_Provider_{request.ProviderCuit ?? "Unknown"}";
        }
        else if (HasRole(request.UserRoles, followingPreloadSocieties))
        {
            return $"Dashboard_Statistics_Societies_{request.UserEmail ?? "Unknown"}";
        }

        return "Dashboard_Statistics_Unknown";
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

    /// <summary>
    /// Builds a predicate expression for filtering documents by society CUITs.
    /// Uses explicit OR conditions to avoid OPENJSON issues with SQL Server.
    /// </summary>
    /// <param name="societyCuits">List of society CUITs to filter by.</param>
    /// <returns>Expression predicate for filtering documents.</returns>
    private static Expression<Func<Document, bool>> BuildSocietyCuitDocumentPredicate(List<string> societyCuits)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(Document), "d");
        MemberExpression property = Expression.Property(parameter, nameof(Document.SociedadCuit));
        MemberExpression fechaEmisionProperty = Expression.Property(parameter, nameof(Document.FechaEmisionComprobante));
        MemberExpression fechaBajaProperty = Expression.Property(parameter, nameof(Document.FechaBaja));
        MemberExpression estadoIdProperty = Expression.Property(parameter, nameof(Document.EstadoId));

        BinaryExpression nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
        MemberExpression fechaEmisionHasValue = Expression.Property(fechaEmisionProperty, "HasValue");
        BinaryExpression fechaBajaIsNull = Expression.Equal(fechaBajaProperty, Expression.Constant(null, typeof(DateTime?)));
        BinaryExpression estadoIdIsNotNull = Expression.NotEqual(estadoIdProperty, Expression.Constant(null, typeof(int?)));

        Expression? orExpression = null;
        foreach (string cuit in societyCuits)
        {
            BinaryExpression equalsExpression = Expression.Equal(property, Expression.Constant(cuit, typeof(string)));
            orExpression = orExpression is null
                ? equalsExpression
                : Expression.OrElse(orExpression, equalsExpression);
        }

        if (orExpression is null)
        {
            // If no CUITs, return false predicate
            return Expression.Lambda<Func<Document, bool>>(Expression.Constant(false), parameter);
        }

        // Combine: FechaBaja == null AND FechaEmisionComprobante.HasValue AND EstadoId != null AND (SociedadCuit != null AND SociedadCuit IN list)
        BinaryExpression societyFilter = Expression.AndAlso(nullCheck, orExpression);
        BinaryExpression fechaEmisionAndSociety = Expression.AndAlso(fechaEmisionHasValue, societyFilter);
        BinaryExpression estadoIdAndFechaEmision = Expression.AndAlso(estadoIdIsNotNull, fechaEmisionAndSociety);
        BinaryExpression combinedExpression = Expression.AndAlso(fechaBajaIsNull, estadoIdAndFechaEmision);
        return Expression.Lambda<Func<Document, bool>>(combinedExpression, parameter);
    }

    /// <summary>
    /// Builds a predicate expression for filtering purchase orders by society CUITs.
    /// Uses explicit OR conditions to avoid OPENJSON issues with SQL Server.
    /// </summary>
    /// <param name="societyCuits">List of society CUITs to filter by.</param>
    /// <returns>Expression predicate for filtering purchase orders.</returns>
    private static Expression<Func<PurchaseOrder, bool>> BuildSocietyCuitPurchaseOrderPredicate(List<string> societyCuits)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(PurchaseOrder), "po");
        MemberExpression documentProperty = Expression.Property(parameter, nameof(PurchaseOrder.Document));
        MemberExpression sociedadCuitProperty = Expression.Property(documentProperty, nameof(Document.SociedadCuit));
        MemberExpression fechaEmisionProperty = Expression.Property(documentProperty, nameof(Document.FechaEmisionComprobante));
        MemberExpression fechaBajaProperty = Expression.Property(documentProperty, nameof(Document.FechaBaja));
        MemberExpression estadoIdProperty = Expression.Property(documentProperty, nameof(Document.EstadoId));

        BinaryExpression nullCheck = Expression.NotEqual(sociedadCuitProperty, Expression.Constant(null, typeof(string)));
        MemberExpression fechaEmisionHasValue = Expression.Property(fechaEmisionProperty, "HasValue");
        BinaryExpression fechaBajaIsNull = Expression.Equal(fechaBajaProperty, Expression.Constant(null, typeof(DateTime?)));
        BinaryExpression estadoIdIsNotNull = Expression.NotEqual(estadoIdProperty, Expression.Constant(null, typeof(int?)));

        Expression? orExpression = null;
        foreach (string cuit in societyCuits)
        {
            BinaryExpression equalsExpression = Expression.Equal(sociedadCuitProperty, Expression.Constant(cuit, typeof(string)));
            orExpression = orExpression is null
                ? equalsExpression
                : Expression.OrElse(orExpression, equalsExpression);
        }

        if (orExpression is null)
        {
            // If no CUITs, return false predicate
            return Expression.Lambda<Func<PurchaseOrder, bool>>(Expression.Constant(false), parameter);
        }

        // Combine: Document.FechaBaja == null AND Document.FechaEmisionComprobante.HasValue AND Document.EstadoId != null AND (Document.SociedadCuit != null AND Document.SociedadCuit IN list)
        BinaryExpression societyFilter = Expression.AndAlso(nullCheck, orExpression);
        BinaryExpression fechaEmisionAndSociety = Expression.AndAlso(fechaEmisionHasValue, societyFilter);
        BinaryExpression estadoIdAndFechaEmision = Expression.AndAlso(estadoIdIsNotNull, fechaEmisionAndSociety);
        BinaryExpression combinedExpression = Expression.AndAlso(fechaBajaIsNull, estadoIdAndFechaEmision);
        return Expression.Lambda<Func<PurchaseOrder, bool>>(combinedExpression, parameter);
    }

    /// <summary>
    /// Builds a predicate expression for filtering pending documents by society CUITs.
    /// Uses explicit OR conditions to avoid OPENJSON issues with SQL Server.
    /// </summary>
    /// <param name="societyCuits">List of society CUITs to filter by.</param>
    /// <returns>Expression predicate for filtering pending documents.</returns>
    private static Expression<Func<Document, bool>> BuildSocietyCuitPendingDocumentPredicate(List<string> societyCuits)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(Document), "d");
        MemberExpression sociedadCuitProperty = Expression.Property(parameter, nameof(Document.SociedadCuit));
        MemberExpression estadoIdProperty = Expression.Property(parameter, nameof(Document.EstadoId));
        MemberExpression fechaEmisionProperty = Expression.Property(parameter, nameof(Document.FechaEmisionComprobante));
        MemberExpression fechaBajaProperty = Expression.Property(parameter, nameof(Document.FechaBaja));

        BinaryExpression nullCheck = Expression.NotEqual(sociedadCuitProperty, Expression.Constant(null, typeof(string)));
        MemberExpression fechaEmisionHasValue = Expression.Property(fechaEmisionProperty, "HasValue");
        BinaryExpression fechaBajaIsNull = Expression.Equal(fechaBajaProperty, Expression.Constant(null, typeof(DateTime?)));

        // Pending state: EstadoId == 1 || EstadoId == 2 || EstadoId == 5
        // EstadoId is nullable, so we need to use nullable int constants
        BinaryExpression estado1Or2 = Expression.OrElse(
            Expression.Equal(estadoIdProperty, Expression.Constant((int?)1, typeof(int?))),
            Expression.Equal(estadoIdProperty, Expression.Constant((int?)2, typeof(int?))));
        BinaryExpression pendingState = Expression.OrElse(
            estado1Or2,
            Expression.Equal(estadoIdProperty, Expression.Constant((int?)5, typeof(int?))));

        Expression? orExpression = null;
        foreach (string cuit in societyCuits)
        {
            BinaryExpression equalsExpression = Expression.Equal(sociedadCuitProperty, Expression.Constant(cuit, typeof(string)));
            orExpression = orExpression is null
                ? equalsExpression
                : Expression.OrElse(orExpression, equalsExpression);
        }

        if (orExpression is null)
        {
            // If no CUITs, return false predicate
            return Expression.Lambda<Func<Document, bool>>(Expression.Constant(false), parameter);
        }

        // Combine: FechaBaja == null AND FechaEmisionComprobante.HasValue AND (SociedadCuit != null) AND (SociedadCuit IN list) AND (EstadoId == 1 || EstadoId == 2 || EstadoId == 5)
        BinaryExpression societyFilter = Expression.AndAlso(nullCheck, orExpression);
        BinaryExpression societyAndPending = Expression.AndAlso(societyFilter, pendingState);
        BinaryExpression fechaEmisionAndSociety = Expression.AndAlso(fechaEmisionHasValue, societyAndPending);
        BinaryExpression combinedExpression = Expression.AndAlso(fechaBajaIsNull, fechaEmisionAndSociety);
        return Expression.Lambda<Func<Document, bool>>(combinedExpression, parameter);
    }

    /// <summary>
    /// Builds a predicate expression for filtering paid documents by society CUITs.
    /// Uses explicit OR conditions to avoid OPENJSON issues with SQL Server.
    /// </summary>
    /// <param name="societyCuits">List of society CUITs to filter by.</param>
    /// <returns>Expression predicate for filtering paid documents.</returns>
    private static Expression<Func<Document, bool>> BuildSocietyCuitPaidDocumentPredicate(List<string> societyCuits)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(Document), "d");
        MemberExpression sociedadCuitProperty = Expression.Property(parameter, nameof(Document.SociedadCuit));
        MemberExpression fechaPagoProperty = Expression.Property(parameter, nameof(Document.FechaPago));
        MemberExpression estadoIdProperty = Expression.Property(parameter, nameof(Document.EstadoId));
        MemberExpression fechaEmisionProperty = Expression.Property(parameter, nameof(Document.FechaEmisionComprobante));
        MemberExpression fechaBajaProperty = Expression.Property(parameter, nameof(Document.FechaBaja));

        BinaryExpression nullCheck = Expression.NotEqual(sociedadCuitProperty, Expression.Constant(null, typeof(string)));
        MemberExpression fechaEmisionHasValue = Expression.Property(fechaEmisionProperty, "HasValue");
        BinaryExpression fechaBajaIsNull = Expression.Equal(fechaBajaProperty, Expression.Constant(null, typeof(DateTime?)));
        BinaryExpression fechaPagoIsNotNull = Expression.NotEqual(fechaPagoProperty, Expression.Constant(null, typeof(DateTime?)));
        // Paid state: EstadoId == 16
        // EstadoId is nullable, so we need to use nullable int constant
        BinaryExpression paidState = Expression.Equal(estadoIdProperty, Expression.Constant((int?)16, typeof(int?)));

        Expression? orExpression = null;
        foreach (string cuit in societyCuits)
        {
            BinaryExpression equalsExpression = Expression.Equal(sociedadCuitProperty, Expression.Constant(cuit, typeof(string)));
            orExpression = orExpression is null
                ? equalsExpression
                : Expression.OrElse(orExpression, equalsExpression);
        }

        if (orExpression is null)
        {
            // If no CUITs, return false predicate
            return Expression.Lambda<Func<Document, bool>>(Expression.Constant(false), parameter);
        }

        // Combine: FechaBaja == null AND FechaEmisionComprobante.HasValue AND FechaPago != null AND EstadoId == 16 AND (SociedadCuit != null) AND (SociedadCuit IN list)
        BinaryExpression societyFilter = Expression.AndAlso(nullCheck, orExpression);
        BinaryExpression societyAndPaidState = Expression.AndAlso(societyFilter, paidState);
        BinaryExpression societyAndPaid = Expression.AndAlso(societyAndPaidState, fechaPagoIsNotNull);
        BinaryExpression fechaEmisionAndSociety = Expression.AndAlso(fechaEmisionHasValue, societyAndPaid);
        BinaryExpression combinedExpression = Expression.AndAlso(fechaBajaIsNull, fechaEmisionAndSociety);
        return Expression.Lambda<Func<Document, bool>>(combinedExpression, parameter);
    }
}

