using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.DocumentStates;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;

internal sealed class DocumentStateRepository : GenericRepository<DocumentState, PreloadDbContext>, IDocumentStateRepository
{
    public DocumentStateRepository(PreloadDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Override GetByIdAsync because DocumentState uses EstDocId (int) as primary key.
    /// </summary>
    public override async Task<DocumentState?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(ds => ds.EstDocId == id && ds.FechaBaja == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Override GetAllAsync to filter out deleted document states (FechaBaja is null).
    /// </summary>
    public override async Task<IEnumerable<DocumentState>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(ds => ds.FechaBaja == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DocumentState>> GetByDocumentIdAsync(int docId, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(ds => ds.DocId == docId && ds.FechaBaja == null)
            .OrderByDescending(ds => ds.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DocumentState>> GetByEstadoIdAsync(int estadoId, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(ds => ds.EstadoId == estadoId && ds.FechaBaja == null)
            .OrderByDescending(ds => ds.FechaCreacion)
            .ToListAsync(cancellationToken);
    }
}
