using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.Currencies;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;

internal sealed class CurrencyRepository : GenericRepository<Currency, PreloadDbContext>, ICurrencyRepository
{
    public CurrencyRepository(PreloadDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Override GetByIdAsync because Currency uses Codigo (string) as primary key, not int.
    /// </summary>
    public override async Task<Currency?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Currency uses Codigo as PK, so we need to find by MonedaId instead
        return await GetQueryable()
            .FirstOrDefaultAsync(c => c.MonedaId == id, cancellationToken);
    }

    public async Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return await GetQueryable()
            .FirstOrDefaultAsync(c => c.Codigo == code, cancellationToken);
    }
}


