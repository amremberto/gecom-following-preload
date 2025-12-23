using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Spd_Sap.SapProviderSocieties;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Spd_Sap;

/// <summary>
/// Repository implementation for SapProviderSocietiy entities.
/// </summary>
internal sealed class SapProviderSocietiyRepository : GenericRepository<SapProviderSocietiy, SpdSapDbContext>, ISapProviderSocietiyRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SapProviderSocietiyRepository"/> class.
    /// </summary>
    /// <param name="context">The SAP database context.</param>
    public SapProviderSocietiyRepository(SpdSapDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetSocietyFiByProviderAccountNumberAsync(string providerAccountNumber, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerAccountNumber);

        List<string> sociedadFiList = await GetQueryable()
            .Where(ps => ps.Proveedor == providerAccountNumber && ps.Sociedadfi != null)
            .Select(ps => ps.Sociedadfi!)
            .Distinct()
            .ToListAsync(cancellationToken);

        return sociedadFiList;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetProviderAccountNumbersBySocietyFiAsync(string societyFi, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(societyFi);

        List<string> providerAccountNumbers = await GetQueryable()
            .Where(ps => ps.Sociedadfi == societyFi && ps.Proveedor != null)
            .Select(ps => ps.Proveedor!)
            .Distinct()
            .ToListAsync(cancellationToken);

        return providerAccountNumbers;
    }

    /// <inheritdoc />
    public override async Task<(IReadOnlyList<SapProviderSocietiy> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        IQueryable<SapProviderSocietiy> query = GetQueryable();

        int totalCount = await query.CountAsync(cancellationToken);

        List<SapProviderSocietiy> items = await query
            .OrderBy(ps => ps.Sociedadfi)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
