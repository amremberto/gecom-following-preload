using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.States;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;

internal sealed class StateRepository : GenericRepository<State, PreloadDbContext>, IStateRepository
{
    public StateRepository(PreloadDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Override GetByIdAsync because State uses EstadoId (int) as primary key.
    /// </summary>
    public override async Task<State?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .FirstOrDefaultAsync(s => s.EstadoId == id, cancellationToken);
    }

    public async Task<State?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return await GetQueryable()
            .FirstOrDefaultAsync(s => s.Codigo == code, cancellationToken);
    }

    public async Task<State?> GetByEstadoIdAsync(int estadoId, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .FirstOrDefaultAsync(s => s.EstadoId == estadoId, cancellationToken);
    }
}
