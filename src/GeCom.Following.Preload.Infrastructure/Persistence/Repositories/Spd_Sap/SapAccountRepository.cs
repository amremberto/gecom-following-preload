using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Spd_Sap.SapAccounts;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Spd_Sap;

internal sealed class SapAccountRepository : GenericRepository<SapAccount, SpdSapDbContext>, ISapAccountRepository
{
    public SapAccountRepository(SpdSapDbContext context) : base(context)
    {
    }

    public async Task<SapAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountNumber);
        return await GetQueryable()
            .FirstOrDefaultAsync(a => a.Accountnumber == accountNumber, cancellationToken);
    }

    public async Task<SapAccount?> GetByAccountNumberForUpdateAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountNumber);
        return await GetTrackedQueryable()
            .FirstOrDefaultAsync(a => a.Accountnumber == accountNumber, cancellationToken);
    }

    public async Task<SapAccount?> GetByCuitAsync(string cuit, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cuit);
        return await GetQueryable()
            .FirstOrDefaultAsync(a => a.NewCuit == cuit, cancellationToken);
    }

    public async Task RemoveByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accountNumber);
        SapAccount? account = await GetTrackedQueryable()
            .FirstOrDefaultAsync(a => a.Accountnumber == accountNumber, cancellationToken);

        if (account is not null)
        {
            await RemoveAsync(account, cancellationToken);
        }
    }

    public override async Task<(IReadOnlyList<SapAccount> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        IQueryable<SapAccount> query = GetQueryable();

        int totalCount = await query.CountAsync(cancellationToken);

        List<SapAccount> items = await query
            .OrderBy(a => a.Accountnumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}

