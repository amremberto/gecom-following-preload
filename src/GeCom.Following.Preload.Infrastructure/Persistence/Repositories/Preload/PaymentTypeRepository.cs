using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.PaymentTypes;
using Microsoft.EntityFrameworkCore;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;

internal sealed class PaymentTypeRepository : GenericRepository<PaymentType, PreloadDbContext>, IPaymentTypeRepository
{
    public PaymentTypeRepository(PreloadDbContext context) : base(context)
    {
    }

    public async Task<PaymentType?> GetByDescripcionAsync(string descripcion, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        return await GetQueryable()
            .FirstOrDefaultAsync(pt => pt.Descripcion == descripcion, cancellationToken);
    }
}
