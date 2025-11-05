using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;

internal sealed class DocumentRepository : GenericRepository<Document, PreloadDbContext>, IDocumentRepository
{
    public DocumentRepository(PreloadDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Document>> GetByProveedorCuitAsync(string proveedorCuit, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(proveedorCuit);
        return await GetQueryable()
            .Where(d => d.ProveedorCuit == proveedorCuit)
            .OrderByDescending(d => d.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetBySociedadCuitAsync(string sociedadCuit, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sociedadCuit);
        return await GetQueryable()
            .Where(d => d.SociedadCuit == sociedadCuit)
            .OrderByDescending(d => d.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetByTipoDocIdAsync(int tipoDocId, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(d => d.TipoDocId == tipoDocId)
            .OrderByDescending(d => d.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetByEstadoIdAsync(int estadoId, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(d => d.EstadoId == estadoId)
            .OrderByDescending(d => d.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetByMonedaAsync(string moneda, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moneda);
        return await GetQueryable()
            .Where(d => d.Moneda == moneda)
            .OrderByDescending(d => d.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetByNumeroComprobanteAsync(string numeroComprobante, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(numeroComprobante);
        return await GetQueryable()
            .Where(d => d.NumeroComprobante == numeroComprobante)
            .OrderByDescending(d => d.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetByCodigoDeBarrasAsync(string codigoDeBarras, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigoDeBarras);
        return await GetQueryable()
            .Where(d => d.CodigoDeBarras == codigoDeBarras)
            .OrderByDescending(d => d.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetByCaecaiAsync(string caecai, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(caecai);
        return await GetQueryable()
            .Where(d => d.Caecai == caecai)
            .OrderByDescending(d => d.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetByUserCreateAsync(string userCreate, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userCreate);
        return await GetQueryable()
            .Where(d => d.UserCreate == userCreate)
            .OrderByDescending(d => d.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(d => d.FechaCreacion >= startDate && d.FechaCreacion <= endDate)
            .OrderByDescending(d => d.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetByAmountRangeAsync(decimal minAmount, decimal maxAmount, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(d => d.MontoBruto >= minAmount && d.MontoBruto <= maxAmount)
            .OrderByDescending(d => d.FechaCreacion)
            .ToListAsync(cancellationToken);
    }
}
