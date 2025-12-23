using System.Linq.Expressions;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.Societies;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;

internal sealed class SocietyRepository : GenericRepository<Society, PreloadDbContext>, ISocietyRepository
{
    public SocietyRepository(PreloadDbContext context) : base(context)
    {
    }

    public async Task<Society?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        return await GetQueryable()
            .FirstOrDefaultAsync(s => s.Codigo == codigo, cancellationToken);
    }

    public async Task<Society?> GetByCuitAsync(string cuit, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cuit);
        return await GetQueryable()
            .FirstOrDefaultAsync(s => s.Cuit == cuit, cancellationToken);
    }

    public async Task<IEnumerable<Society>> GetByCuitsAsync(IReadOnlyList<string> cuits, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cuits);

        if (cuits.Count == 0)
        {
            return [];
        }

        // Filter out empty/null CUITs
        var validCuits = cuits
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .ToList();

        if (validCuits.Count == 0)
        {
            return [];
        }

        // Build explicit OR conditions to avoid OPENJSON issues with SQL Server
        // EF Core uses OPENJSON for Contains() which fails on some SQL Server versions
        // See documentation in docs/SQL-SERVER-OPENJSON-ISSUE.md
        // This generates SQL like: WHERE Cuit = 'VAL1' OR Cuit = 'VAL2' OR Cuit = 'VAL3'
        // Instead of: WHERE Cuit IN (SELECT value FROM OPENJSON('["VAL1","VAL2","VAL3"]') WITH ([value] varchar(12) '$'))
        ParameterExpression parameter = Expression.Parameter(typeof(Society), "s");
        MemberExpression property = Expression.Property(parameter, nameof(Society.Cuit));
        BinaryExpression nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));

        Expression? orExpression = null;
        foreach (string cuit in validCuits)
        {
            BinaryExpression equalsExpression = Expression.Equal(property, Expression.Constant(cuit, typeof(string)));
            orExpression = orExpression is null
                ? equalsExpression
                : Expression.OrElse(orExpression, equalsExpression);
        }

        if (orExpression is null)
        {
            return [];
        }

        // Combine null check with OR expression
        Expression combinedExpression = Expression.AndAlso(nullCheck, orExpression);
        var lambda = Expression.Lambda<Func<Society, bool>>(combinedExpression, parameter);

        List<Society> societies = await GetQueryable()
            .Where(lambda)
            .ToListAsync(cancellationToken);

        return societies;
    }

    public override async Task<(IReadOnlyList<Society> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        IQueryable<Society> query = GetQueryable();

        int totalCount = await query.CountAsync(cancellationToken);

        List<Society> items = await query
            .OrderBy(s => s.SocId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
