using System.Linq.Expressions;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;

internal sealed class DocumentRepository : GenericRepository<Document, PreloadDbContext>, IDocumentRepository
{
    public DocumentRepository(PreloadDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public override async Task<Document?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetTrackedQueryable()
            .Include(d => d.Provider)
            .Include(d => d.Society)
            .Include(d => d.DocumentType)
            .Include(d => d.State)
            .Include(d => d.PurchaseOrders)
            .Include(d => d.Notes)
            .Include(d => d.Attachments.Where(a => a.FechaBorrado == null))
            .FirstOrDefaultAsync(d => d.DocId == id, cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<Document>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Include(d => d.Provider)
            .Include(d => d.Society)
            .Include(d => d.DocumentType)
            .Include(d => d.State)
            .Include(d => d.PurchaseOrders)
            .Include(d => d.Notes)
            .Where(d => d.EstadoId != null && d.EstadoId != 1 && d.EstadoId != 2 && d.EstadoId != 5)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
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

    public async Task<IEnumerable<Document>> GetByEmissionDatesAndProviderCuitAsync(DateOnly dateFrom, DateOnly dateTo, string? providerCuit, CancellationToken cancellationToken = default)
    {
        IQueryable<Document> query = GetQueryable()
            .Include(d => d.Provider)
            .Include(d => d.Society)
            .Include(d => d.DocumentType)
            .Include(d => d.State)
            .Include(d => d.PurchaseOrders)
            .Include(d => d.Notes)
            .Where(d => d.FechaEmisionComprobante.HasValue
                && d.FechaEmisionComprobante >= dateFrom
                && d.FechaEmisionComprobante <= dateTo
                && d.EstadoId != null
                && d.EstadoId != 1
                && d.EstadoId != 2
                && d.EstadoId != 5);

        if (!string.IsNullOrWhiteSpace(providerCuit))
        {
            query = query.Where(d => d.ProveedorCuit == providerCuit);
        }

        return await query
            .OrderByDescending(d => d.FechaEmisionComprobante)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Document>> GetByEmissionDatesAndSocietyCuitsAsync(DateOnly dateFrom, DateOnly dateTo, IEnumerable<string> societyCuits, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(societyCuits);

        IQueryable<Document> query = GetQueryable()
            .Include(d => d.Provider)
            .Include(d => d.Society)
            .Include(d => d.DocumentType)
            .Include(d => d.State)
            .Include(d => d.PurchaseOrders)
            .Include(d => d.Notes)
            .Where(d => d.FechaEmisionComprobante.HasValue
                && d.FechaEmisionComprobante >= dateFrom
                && d.FechaEmisionComprobante <= dateTo
                && d.EstadoId != null
                && d.EstadoId != 1
                && d.EstadoId != 2
                && d.EstadoId != 5);

        var societyCuitsList = societyCuits
            .Where(cuit => !string.IsNullOrWhiteSpace(cuit))
            .Distinct()
            .ToList();

        if (societyCuitsList.Count > 0)
        {
            // Build explicit OR conditions to avoid OPENJSON issues with SQL Server
            // EF Core uses OPENJSON for Contains() which fails on some SQL Server versions
            // Build OR expression tree manually to ensure SQL translation works correctly
            ParameterExpression? parameter = Expression.Parameter(typeof(Document), "d");
            MemberExpression? property = Expression.Property(parameter, nameof(Document.SociedadCuit));
            BinaryExpression? nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));

            Expression? orExpression = null;
            foreach (string cuit in societyCuitsList)
            {
                BinaryExpression? equalsExpression = Expression.Equal(property, Expression.Constant(cuit, typeof(string)));
                orExpression = orExpression is null
                    ? equalsExpression
                    : Expression.OrElse(orExpression, equalsExpression);
            }

            if (orExpression is not null)
            {
                BinaryExpression? combinedExpression = Expression.AndAlso(nullCheck, orExpression);
                var lambda = Expression.Lambda<Func<Document, bool>>(combinedExpression, parameter);
                query = query.Where(lambda);
            }
        }

        return await query
            .OrderByDescending(d => d.FechaEmisionComprobante)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Document>> GetPendingByProviderCuitAsync(string providerCuit, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(providerCuit);

        return await GetQueryable()
            .Include(d => d.Provider)
            .Include(d => d.Society)
            .Include(d => d.DocumentType)
            .Include(d => d.State)
            .Include(d => d.PurchaseOrders)
            .Include(d => d.Notes)
            .Where(d => d.FechaEmisionComprobante.HasValue
                && d.ProveedorCuit == providerCuit
                && (d.EstadoId == 1 || d.EstadoId == 2 || d.EstadoId == 5))
            .OrderBy(d => d.FechaEmisionComprobante)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Document>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await GetQueryable()
            .Include(d => d.Provider)
            .Include(d => d.Society)
            .Include(d => d.DocumentType)
            .Include(d => d.State)
            .Include(d => d.PurchaseOrders)
            .Include(d => d.Notes)
            .Where(d => d.FechaEmisionComprobante.HasValue
                && (d.EstadoId == 1 || d.EstadoId == 2 || d.EstadoId == 5))
            .OrderBy(d => d.FechaEmisionComprobante)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Document>> GetPendingBySocietyCuitsAsync(IEnumerable<string> societyCuits, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(societyCuits);

        IQueryable<Document> query = GetQueryable()
            .Include(d => d.Provider)
            .Include(d => d.Society)
            .Include(d => d.DocumentType)
            .Include(d => d.State)
            .Include(d => d.PurchaseOrders)
            .Include(d => d.Notes)
            .Where(d => d.FechaEmisionComprobante.HasValue
                && (d.EstadoId == 1 || d.EstadoId == 2 || d.EstadoId == 5));

        var societyCuitsList = societyCuits
            .Where(cuit => !string.IsNullOrWhiteSpace(cuit))
            .Distinct()
            .ToList();

        if (societyCuitsList.Count > 0)
        {
            // Build explicit OR conditions to avoid OPENJSON issues with SQL Server
            // EF Core uses OPENJSON for Contains() which fails on some SQL Server versions
            // Build OR expression tree manually to ensure SQL translation works correctly
            ParameterExpression parameter = Expression.Parameter(typeof(Document), "d");
            MemberExpression property = Expression.Property(parameter, nameof(Document.SociedadCuit));
            BinaryExpression nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));

            Expression? orExpression = null;
            foreach (string cuit in societyCuitsList)
            {
                BinaryExpression equalsExpression = Expression.Equal(property, Expression.Constant(cuit, typeof(string)));
                orExpression = orExpression is null
                    ? equalsExpression
                    : Expression.OrElse(orExpression, equalsExpression);
            }

            if (orExpression is not null)
            {
                BinaryExpression combinedExpression = Expression.AndAlso(nullCheck, orExpression);
                var lambda = Expression.Lambda<Func<Document, bool>>(combinedExpression, parameter);
                query = query.Where(lambda);
            }
        }
        else
        {
            // If no CUITs provided, return empty result
            return Enumerable.Empty<Document>();
        }

        return await query
            .OrderBy(d => d.FechaEmisionComprobante)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
