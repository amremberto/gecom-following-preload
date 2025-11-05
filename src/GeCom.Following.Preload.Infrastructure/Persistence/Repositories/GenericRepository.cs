using System.Linq.Expressions;
using GeCom.Following.Preload.SharedKernel.Entities;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository implementation providing CRUD operations for domain entities.
/// </summary>
/// <typeparam name="TEntity">The type of entity this repository manages.</typeparam>
internal class GenericRepository<TEntity, TContext> : IRepository<TEntity>
    where TEntity : BaseEntity
    where TContext : DbContext
{
    protected readonly TContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericRepository{TEntity}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public GenericRepository(TContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await _dbSet
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await _dbSet
            .AnyAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        EntityEntry<TEntity>? entry = await _dbSet
            .AddAsync(entity, cancellationToken);

        return entry.Entity;
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var entitiesList = entities.ToList();
        await _dbSet
            .AddRangeAsync(entitiesList, cancellationToken);

        return entitiesList;
    }

    /// <inheritdoc />
    public virtual Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet
            .Update(entity);

        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public virtual Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        var entitiesList = entities.ToList();
        _dbSet
            .UpdateRange(entitiesList);

        return Task.FromResult<IEnumerable<TEntity>>(entitiesList);
    }

    /// <inheritdoc />
    public virtual Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet
            .Remove(entity);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        _dbSet
            .RemoveRange(entities);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual async Task RemoveByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        TEntity? entity = await GetByIdAsync(id, cancellationToken);

        if (entity is not null)
        {
            await RemoveAsync(entity, cancellationToken);
        }
    }

    /// <summary>
    /// Gets a queryable for the entity set with no tracking.
    /// </summary>
    /// <returns>A queryable for read-only operations.</returns>
    protected IQueryable<TEntity> GetQueryable()
    {
        return _dbSet.AsNoTracking();
    }

    /// <summary>
    /// Gets a queryable for the entity set with tracking enabled.
    /// </summary>
    /// <returns>A queryable for write operations.</returns>
    protected IQueryable<TEntity> GetTrackedQueryable()
    {
        return _dbSet;
    }

    /// <summary>
    /// Gets the DbSet for advanced operations.
    /// </summary>
    /// <returns>The DbSet for the entity.</returns>
    protected DbSet<TEntity> GetDbSet()
    {
        return _dbSet;
    }
}
