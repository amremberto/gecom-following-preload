using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.Notes;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;

internal sealed class NoteRepository : GenericRepository<Note, PreloadDbContext>, INoteRepository
{
    public NoteRepository(PreloadDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Override GetByIdAsync because Note uses NotaId (int) as primary key.
    /// </summary>
    public override async Task<Note?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .FirstOrDefaultAsync(n => n.NotaId == id, cancellationToken);
    }

    public async Task<Note?> GetByNotaIdAsync(int notaId, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .FirstOrDefaultAsync(n => n.NotaId == notaId, cancellationToken);
    }

    public async Task<IEnumerable<Note>> GetByDocumentIdAsync(int docId, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(n => n.DocId == docId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Note>> GetByUsuarioCreacionAsync(string usuarioCreacion, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(usuarioCreacion);
        return await GetQueryable()
            .Where(n => n.UsuarioCreacion == usuarioCreacion)
            .ToListAsync(cancellationToken);
    }
}
