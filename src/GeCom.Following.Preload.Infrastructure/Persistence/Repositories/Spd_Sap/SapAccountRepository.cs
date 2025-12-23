using System.Linq.Expressions;
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

    /// <inheritdoc />
    public async Task<IReadOnlyList<SapAccount>> GetByAccountNumbersAndCustomerTypeAsync(
        IReadOnlyList<string> accountNumbers,
        int customerTypeCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accountNumbers);

        if (accountNumbers.Count == 0)
        {
            return [];
        }

        // Filter out empty account numbers
        var validAccountNumbers = accountNumbers
            .Where(an => !string.IsNullOrWhiteSpace(an))
            .Distinct()
            .ToList();

        if (validAccountNumbers.Count == 0)
        {
            return [];
        }

        // Process in batches to avoid StackOverflowException when there are too many account numbers
        // Large OR expressions can cause stack overflow, so we process in chunks
        const int batchSize = 100;
        var allAccounts = new List<SapAccount>();

        for (int i = 0; i < validAccountNumbers.Count; i += batchSize)
        {
            var batch = validAccountNumbers.Skip(i).Take(batchSize).ToList();

            // Build query base with customer type and CUIT filters
            IQueryable<SapAccount> query = GetQueryable()
                .Where(a => a.Customertypecode == customerTypeCode
                    && a.NewCuit != null
                    && !string.IsNullOrWhiteSpace(a.NewCuit));

            // Build explicit OR conditions to avoid OPENJSON issues with SQL Server
            // EF Core uses OPENJSON for Contains() which fails on some SQL Server versions
            // See documentation in docs/SQL-SERVER-OPENJSON-ISSUE.md
            ParameterExpression parameter = Expression.Parameter(typeof(SapAccount), "a");
            MemberExpression property = Expression.Property(parameter, nameof(SapAccount.Accountnumber));
            BinaryExpression nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));

            Expression? orExpression = null;
            foreach (string accountNumber in batch)
            {
                BinaryExpression equalsExpression = Expression.Equal(property, Expression.Constant(accountNumber, typeof(string)));
                orExpression = orExpression is null
                    ? equalsExpression
                    : Expression.OrElse(orExpression, equalsExpression);
            }

            if (orExpression is not null)
            {
                BinaryExpression combinedExpression = Expression.AndAlso(nullCheck, orExpression);
                var lambda = Expression.Lambda<Func<SapAccount, bool>>(combinedExpression, parameter);
                query = query.Where(lambda);

                List<SapAccount> batchAccounts = await query.ToListAsync(cancellationToken);
                allAccounts.AddRange(batchAccounts);
            }
        }

        // Remove duplicates (in case same account appears in multiple batches)
        return allAccounts
            .GroupBy(a => a.Accountnumber)
            .Select(g => g.First())
            .ToList();
    }
}

