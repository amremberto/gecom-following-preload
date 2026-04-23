using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.PurchaseOrders;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;

internal sealed class PurchaseOrderRepository : GenericRepository<PurchaseOrder, PreloadDbContext>, IPurchaseOrderRepository
{
    public PurchaseOrderRepository(PreloadDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PurchaseOrder>> GetByDocumentIdAsync(int documentId, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(po => po.DocId == documentId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PurchaseOrder>> GetByProviderSapCodeAsync(string providerSapCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerSapCode);

        return await GetQueryable()
            .Where(po => po.ProveedorSap == providerSapCode)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PurchaseOrder>> GetBySocietyCodeAsync(string societyCode, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(societyCode);

        return await GetQueryable()
            .Where(po => po.CodigoSociedadFi == societyCode)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PurchaseOrder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Where(po => po.FechaCreacion >= startDate && po.FechaCreacion <= endDate)
            .OrderBy(po => po.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PurchaseOrder>> GetWithDocumentsAsync(CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Include(po => po.Document)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsActiveLinkAsync(
        int docId,
        int ordenCompraId,
        string nroOc,
        int posicionOc,
        string codigoSociedadFi,
        string proveedorSap,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nroOc);
        ArgumentException.ThrowIfNullOrWhiteSpace(codigoSociedadFi);
        ArgumentException.ThrowIfNullOrWhiteSpace(proveedorSap);

        string normalizedNroOc = nroOc.Trim();
        string normalizedSocietyCode = codigoSociedadFi.Trim();
        string normalizedProviderSap = proveedorSap.Trim();

        return await GetQueryable()
            .AnyAsync(po =>
                    po.DocId == docId &&
                    po.OrdenCompraId == ordenCompraId &&
                    po.NroOc == normalizedNroOc &&
                    po.PosicionOc == posicionOc &&
                    po.CodigoSociedadFi == normalizedSocietyCode &&
                    po.ProveedorSap == normalizedProviderSap &&
                    po.FechaBaja == null,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PurchaseOrder?> GetActiveLinkForUnlinkAsync(
        int docId,
        string numeroDocumento,
        int posicion,
        string codigoRecepcion,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(numeroDocumento);
        ArgumentException.ThrowIfNullOrWhiteSpace(codigoRecepcion);

        string normalizedNumeroDocumento = numeroDocumento.Trim();
        string normalizedCodigoRecepcion = codigoRecepcion.Trim();

        return await GetQueryable()
            .FirstOrDefaultAsync(po =>
                    po.DocId == docId &&
                    po.NroOc == normalizedNumeroDocumento &&
                    po.PosicionOc == posicion &&
                    po.CodigoRecepcion == normalizedCodigoRecepcion &&
                    po.FechaBaja == null,
                cancellationToken);
    }
}
