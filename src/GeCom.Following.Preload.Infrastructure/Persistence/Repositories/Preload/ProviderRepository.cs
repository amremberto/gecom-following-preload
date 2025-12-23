using System.Linq.Expressions;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.Providers;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;

internal sealed class ProviderRepository : GenericRepository<Provider, PreloadDbContext>, IProviderRepository
{
    public ProviderRepository(PreloadDbContext context) : base(context)
    {
    }

    public async Task<Provider?> GetByCuitAsync(string cuit, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cuit);
        return await GetQueryable()
            .FirstOrDefaultAsync(p => p.Cuit == cuit, cancellationToken);
    }

    public override async Task<(IReadOnlyList<Provider> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        IQueryable<Provider> query = GetQueryable();

        int totalCount = await query.CountAsync(cancellationToken);

        List<Provider> items = await query
            .OrderBy(p => p.ProvId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<Provider>> SearchAsync(string searchText, int maxResults = 20, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return [];
        }

        string searchPattern = $"%{searchText.Trim()}%";

        return await GetQueryable()
            .Where(p =>
                EF.Functions.Like(p.RazonSocial, searchPattern) ||
                EF.Functions.Like(p.Cuit, searchPattern))
            .OrderBy(p => p.RazonSocial)
            .ThenBy(p => p.Cuit)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Provider>> GetByCuitsAsync(IReadOnlyList<string> cuits, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cuits);

        if (cuits.Count == 0)
        {
            return [];
        }

        // Filter out empty CUITs
        var validCuits = cuits
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .ToList();

        if (validCuits.Count == 0)
        {
            return [];
        }

        // Process in batches to avoid StackOverflowException when there are too many CUITs
        // Large OR expressions can cause stack overflow, so we process in chunks
        const int batchSize = 100;
        var allProviders = new List<Provider>();

        for (int i = 0; i < validCuits.Count; i += batchSize)
        {
            var batch = validCuits.Skip(i).Take(batchSize).ToList();

            // Build explicit OR conditions to avoid OPENJSON issues with SQL Server
            // EF Core uses OPENJSON for Contains() which fails on some SQL Server versions
            // See documentation in docs/SQL-SERVER-OPENJSON-ISSUE.md
            ParameterExpression parameter = Expression.Parameter(typeof(Provider), "p");
            MemberExpression property = Expression.Property(parameter, nameof(Provider.Cuit));
            BinaryExpression nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));

            Expression? orExpression = null;
            foreach (string cuit in batch)
            {
                BinaryExpression equalsExpression = Expression.Equal(property, Expression.Constant(cuit, typeof(string)));
                orExpression = orExpression is null
                    ? equalsExpression
                    : Expression.OrElse(orExpression, equalsExpression);
            }

            if (orExpression is not null)
            {
                BinaryExpression combinedExpression = Expression.AndAlso(nullCheck, orExpression);
                var lambda = Expression.Lambda<Func<Provider, bool>>(combinedExpression, parameter);
                IQueryable<Provider> query = GetQueryable().Where(lambda);
                List<Provider> batchProviders = await query.ToListAsync(cancellationToken);
                allProviders.AddRange(batchProviders);
            }
        }

        // Remove duplicates (in case same provider appears in multiple batches)
        return allProviders
            .GroupBy(p => p.Cuit)
            .Select(g => g.First())
            .ToList();
    }
}
