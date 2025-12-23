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

        List<Society> societies = await GetQueryable()
            .Where(s => validCuits.Contains(s.Cuit))
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
