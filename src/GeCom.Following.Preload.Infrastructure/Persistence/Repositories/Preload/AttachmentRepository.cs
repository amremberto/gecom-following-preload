using GeCom.Following.Preload.Application.Preload.Attachments.Interfaces;
using GeCom.Following.Preload.Domain.Preloads.Attachments;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;

internal sealed class AttachmentRepository : GenericRepository<Attachment, PreloadDbContext>, IAttachmentRepository
{
    public AttachmentRepository(PreloadDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Attachment>> GetByDocumentIdAsync(int docId, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(a => a.DocId == docId)
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Attachment>> GetByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return await GetQueryable()
            .Where(a => a.Path == path)
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Attachment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(a => a.FechaCreacion >= startDate && a.FechaCreacion <= endDate)
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync(cancellationToken);
    }
}
