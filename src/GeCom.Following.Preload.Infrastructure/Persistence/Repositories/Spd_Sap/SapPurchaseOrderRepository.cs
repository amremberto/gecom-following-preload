using System.Linq.Expressions;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Spd_Sap.SapPurchaseOrders;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Spd_Sap;

/// <summary>
/// Repository implementation for SapPurchaseOrder entities.
/// </summary>
internal sealed class SapPurchaseOrderRepository : GenericRepository<SapPurchaseOrder, SpdSapDbContext>, ISapPurchaseOrderRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SapPurchaseOrderRepository"/> class.
    /// </summary>
    /// <param name="context">The SAP database context.</param>
    public SapPurchaseOrderRepository(SpdSapDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SapPurchaseOrder>> GetByProviderAccountNumberAsync(string providerAccountNumber, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerAccountNumber);

        return await GetQueryable()
            .Where(po => po.Proveedor == providerAccountNumber)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SapPurchaseOrder>> GetBySociedadFiCodesAsync(IReadOnlyList<string> sociedadFiCodes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sociedadFiCodes);

        if (sociedadFiCodes.Count == 0)
        {
            return [];
        }

        // Build explicit OR conditions to avoid OPENJSON issues with SQL Server
        // EF Core uses OPENJSON for Contains() which fails on some SQL Server versions
        // See documentation in docs/SQL-SERVER-OPENJSON-ISSUE.md
        ParameterExpression parameter = Expression.Parameter(typeof(SapPurchaseOrder), "po");
        MemberExpression property = Expression.Property(parameter, nameof(SapPurchaseOrder.Codigosociedadfi));
        BinaryExpression nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));

        Expression? orExpression = null;
        foreach (string sociedadFiCode in sociedadFiCodes)
        {
            if (string.IsNullOrWhiteSpace(sociedadFiCode))
            {
                continue;
            }

            BinaryExpression equalsExpression = Expression.Equal(property, Expression.Constant(sociedadFiCode, typeof(string)));
            orExpression = orExpression is null
                ? equalsExpression
                : Expression.OrElse(orExpression, equalsExpression);
        }

        if (orExpression is not null)
        {
            BinaryExpression combinedExpression = Expression.AndAlso(nullCheck, orExpression);
            var lambda = Expression.Lambda<Func<SapPurchaseOrder, bool>>(combinedExpression, parameter);
            return await GetQueryable()
                .Where(lambda)
                .ToListAsync(cancellationToken);
        }

        return [];
    }

    /// <inheritdoc />
    public override async Task<(IReadOnlyList<SapPurchaseOrder> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        IQueryable<SapPurchaseOrder> query = GetQueryable();

        int totalCount = await query.CountAsync(cancellationToken);

        List<SapPurchaseOrder> items = await query
            .OrderBy(po => po.Idorden)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
