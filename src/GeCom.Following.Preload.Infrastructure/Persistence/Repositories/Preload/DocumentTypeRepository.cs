using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.DocumentTypes;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;

internal sealed class DocumentTypeRepository : GenericRepository<DocumentType, PreloadDbContext>, IDocumentTypeRepository
{
    public DocumentTypeRepository(PreloadDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Override GetByIdAsync because DocumentType uses TipoDocId (int) as primary key.
    /// </summary>
    public override async Task<DocumentType?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .FirstOrDefaultAsync(dt => dt.TipoDocId == id, cancellationToken);
    }

    public async Task<DocumentType?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return await GetQueryable()
            .FirstOrDefaultAsync(dt => dt.Codigo == code, cancellationToken);
    }

    public async Task<DocumentType?> GetByTipoDocIdAsync(int tipoDocId, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .FirstOrDefaultAsync(dt => dt.TipoDocId == tipoDocId, cancellationToken);
    }

    public Task<bool> IsCreditOrDebitNoteAsync(int? tipoDocId, CancellationToken cancellationToken = default)
    {
        // Assuming that credit or debit notes have the field IsCreditOrDebitNote set to true
        return GetQueryable()
            .Where(dt => dt.TipoDocId == tipoDocId)
            .Select(dt => dt.IsNotaDebitoCredito)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
