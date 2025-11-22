using GeCom.Following.Preload.Application.Preload.Attachments.Interfaces;
using GeCom.Following.Preload.Domain.Preloads.Attachments;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;

internal sealed class AttachmentRepository : GenericRepository<Attachment, PreloadDbContext>, IAttachmentRepository
{
    public AttachmentRepository(PreloadDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Override GetByIdAsync because Attachment uses AdjuntoId (int) as primary key.
    /// </summary>
    public override async Task<Attachment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(a => a.AdjuntoId == id && a.FechaBorrado == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Override GetAllAsync to filter out deleted attachments (FechaBorrado is null).
    /// </summary>
    public override async Task<IEnumerable<Attachment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(a => a.FechaBorrado == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Attachment>> GetByDocumentIdAsync(int docId, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(a => a.DocId == docId && a.FechaBorrado == null)
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Attachment>> GetByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return await GetQueryable()
            .Where(a => a.Path == path && a.FechaBorrado == null)
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Attachment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(a => a.FechaCreacion >= startDate && a.FechaCreacion <= endDate && a.FechaBorrado == null)
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync(cancellationToken);
    }
}
