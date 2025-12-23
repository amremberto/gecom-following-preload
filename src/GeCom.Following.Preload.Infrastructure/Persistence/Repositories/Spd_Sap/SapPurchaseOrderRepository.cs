using System.Globalization;
using System.Linq.Expressions;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.DTOs;
using GeCom.Following.Preload.Domain.Spd_Sap.SapPurchaseOrders;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Spd_Sap;

/// <summary>
/// Repository implementation for SapPurchaseOrder entities.
/// </summary>
internal sealed class SapPurchaseOrderRepository : GenericRepository<SapPurchaseOrder, SpdSapDbContext>, ISapPurchaseOrderRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SapPurchaseOrderRepository"/> class.
    /// </summary>
    /// <param name="context">The SAP database context.</param>
    public SapPurchaseOrderRepository(SpdSapDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SapPurchaseOrder>> GetByProviderAccountNumberAsync(string providerAccountNumber, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerAccountNumber);

        return await GetQueryable()
            .Where(po => po.Proveedor == providerAccountNumber)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SapPurchaseOrder>> GetBySociedadFiCodesAsync(IReadOnlyList<string> sociedadFiCodes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sociedadFiCodes);

        if (sociedadFiCodes.Count == 0)
        {
            return [];
        }

        // Build explicit OR conditions to avoid OPENJSON issues with SQL Server
        // EF Core uses OPENJSON for Contains() which fails on some SQL Server versions
        // See documentation in docs/SQL-SERVER-OPENJSON-ISSUE.md
        ParameterExpression parameter = Expression.Parameter(typeof(SapPurchaseOrder), "po");
        MemberExpression property = Expression.Property(parameter, nameof(SapPurchaseOrder.Codigosociedadfi));
        BinaryExpression nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));

        Expression? orExpression = null;
        foreach (string sociedadFiCode in sociedadFiCodes)
        {
            if (string.IsNullOrWhiteSpace(sociedadFiCode))
            {
                continue;
            }

            BinaryExpression equalsExpression = Expression.Equal(property, Expression.Constant(sociedadFiCode, typeof(string)));
            orExpression = orExpression is null
                ? equalsExpression
                : Expression.OrElse(orExpression, equalsExpression);
        }

        if (orExpression is not null)
        {
            BinaryExpression combinedExpression = Expression.AndAlso(nullCheck, orExpression);
            var lambda = Expression.Lambda<Func<SapPurchaseOrder, bool>>(combinedExpression, parameter);
            return await GetQueryable()
                .Where(lambda)
                .ToListAsync(cancellationToken);
        }

        return [];
    }

    /// <inheritdoc />
    public override async Task<(IReadOnlyList<SapPurchaseOrder> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

        IQueryable<SapPurchaseOrder> query = GetQueryable();

        int totalCount = await query.CountAsync(cancellationToken);

        List<SapPurchaseOrder> items = await query
            .OrderBy(po => po.Idorden)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Obtiene los datos crudos de órdenes de compra para notas de crédito/débito.
    /// Este método devuelve el DTO completo con todos los campos necesarios para aplicar la lógica de negocio en el handler.
    /// </summary>
    /// <param name="providerCuit">El CUIT del proveedor.</param>
    /// <param name="societyCode">El código de la sociedad.</param>
    /// <param name="docId">El ID del documento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Una colección de DTOs con los datos crudos de las órdenes de compra.</returns>
    public async Task<IEnumerable<SapPurchaseOrderCreditDebitNoteDto>> GetCreditDebitNoteDataAsync(
        string providerCuit,
        string societyCode,
        int docId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerCuit);
        ArgumentException.ThrowIfNullOrWhiteSpace(societyCode);

        FormattableString sql = $@"
            SELECT
                o.idorden AS OrdenCompraId,
                o.nrodocumento AS NumeroDocumento,
                o.posicion AS Posicion,
                o.textobreve AS DescripcionProducto,
                o.unidad_cp AS UnidadMedida,
                o.fechadocumento AS FechaEmisionOC,
                o.cantidadpedida AS CantidadPedida,
                o.cantidadentregada AS CantidadRecepcionada,
                o.cantidadfacturada AS CantidadFacturada,
                po.CantidadAFacturar AS CantidadAFacturar,
                po.CodigoRecepcion AS CodigoRecepcion,
                CAST(o.importeoriginal / NULLIF(o.cantidadpedida, 0) AS decimal(18, 3)) AS ImporteOriginal,
                o.codigosociedadfi AS CodigoSociedadFI,
                o.proveedor AS ProveedorSAP,
                o.usuario AS Contacto,
                o.NetoAnticipo AS NetoAnticipo
            FROM ordenes o
            LEFT JOIN cuenta c 
                ON o.proveedor = c.accountnumber
            LEFT JOIN Precarga.dbo.OrdenesCompra po
                ON po.codigoSociedadFI = o.codigosociedadfi
                   AND po.proveedorSAP = o.proveedor
                   AND po.NroOC = o.nrodocumento
                   AND po.posicionOC = o.posicion
                   AND po.FechaBaja IS NULL
                   AND po.DocId = {docId}
            WHERE o.codigosociedadfi = {societyCode}
                  AND c.new_cuit = {providerCuit}
                  AND o.fechadocumento >= CONVERT(date, DATEADD(month, -6, GETDATE()))";

        List<SapPurchaseOrderCreditDebitNoteDto> results = await _context
            .Database
            .SqlQuery<SapPurchaseOrderCreditDebitNoteDto>(sql)
            .ToListAsync(cancellationToken);

        return results;
    }

    /// <summary>
    /// Obtiene los datos crudos de órdenes de compra para documentos estándar (no notas de crédito/débito).
    /// Este método devuelve el DTO completo con todos los campos necesarios para aplicar la lógica de negocio en el handler.
    /// Incluye filtros específicos: cantidad facturada menor que entregada, y filtro de fecha de 3 años.
    /// </summary>
    /// <param name="providerCuit">El CUIT del proveedor.</param>
    /// <param name="societyCode">El código de la sociedad.</param>
    /// <param name="docId">El ID del documento.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Una colección de DTOs con los datos crudos de las órdenes de compra.</returns>
    public async Task<IEnumerable<SapPurchaseOrderCreditDebitNoteDto>> GetStandardPurchaseOrderDataAsync(
        string providerCuit,
        string societyCode,
        int docId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerCuit);
        ArgumentException.ThrowIfNullOrWhiteSpace(societyCode);

        FormattableString sql = $@"
            SELECT
                o.idorden AS OrdenCompraId,
                o.nrodocumento AS NumeroDocumento,
                o.posicion AS Posicion,
                o.textobreve AS DescripcionProducto,
                o.unidad_cp AS UnidadMedida,
                o.fechadocumento AS FechaEmisionOC,
                o.cantidadpedida AS CantidadPedida,
                o.cantidadentregada AS CantidadRecepcionada,
                o.cantidadfacturada AS CantidadFacturada,
                po.CantidadAFacturar AS CantidadAFacturar,
                po.CodigoRecepcion AS CodigoRecepcion,
                CAST(o.importeoriginal / NULLIF(o.cantidadpedida, 0) AS decimal(18, 3)) AS ImporteOriginal,
                o.codigosociedadfi AS CodigoSociedadFI,
                o.proveedor AS ProveedorSAP,
                o.usuario AS Contacto,
                o.NetoAnticipo AS NetoAnticipo
            FROM ordenes o
            LEFT JOIN cuenta c 
                ON o.proveedor = c.accountnumber
            LEFT JOIN Precarga.dbo.OrdenesCompra po
                ON po.codigoSociedadFI = o.codigosociedadfi
                   AND po.proveedorSAP = o.proveedor
                   AND po.NroOC = o.nrodocumento
                   AND po.posicionOC = o.posicion
                   AND po.FechaBaja IS NULL
                   AND po.DocId = {docId}
            WHERE o.codigosociedadfi = {societyCode}
                  AND c.new_cuit = {providerCuit}
                  AND (o.cantidadfacturada < o.cantidadentregada 
                       OR (o.cantidadfacturada = o.cantidadentregada 
                           AND o.cantidadentregada < o.cantidadpedida))
                  AND o.fechadocumento >= CONVERT(date, DATEADD(year, -3, GETDATE()))";

        List<SapPurchaseOrderCreditDebitNoteDto> results = await _context
            .Database
            .SqlQuery<SapPurchaseOrderCreditDebitNoteDto>(sql)
            .ToListAsync(cancellationToken);

        return results;
    }
}
