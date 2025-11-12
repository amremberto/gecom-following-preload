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

        searchText = searchText.Trim().ToUpperInvariant();

        return await GetQueryable()
            .Where(p =>
                p.RazonSocial.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                p.Cuit.Contains(searchText, StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(p => p.RazonSocial)
            .ThenBy(p => p.Cuit)
            .Take(maxResults)
            .ToListAsync(cancellationToken);

    }
}

