using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.UserSocietyAssignments;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;

/// <summary>
/// Repository implementation for UserSocietyAssignment entities.
/// </summary>
internal sealed class UserSocietyAssignmentRepository : GenericRepository<UserSocietyAssignment, PreloadDbContext>, IUserSocietyAssignmentRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserSocietyAssignmentRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UserSocietyAssignmentRepository(PreloadDbContext context) : base(context)
    {
    }

    public override async Task<(IReadOnlyList<UserSocietyAssignment> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        IQueryable<UserSocietyAssignment> query = GetQueryable();

        int totalCount = await query.CountAsync(cancellationToken);

        List<UserSocietyAssignment> items = await query
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}

