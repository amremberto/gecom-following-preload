using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Domain.Preloads.PaymentDetails;

namespace GeCom.Following.Preload.Infrastructure.Persistence.Repositories.Preload;

internal sealed class PaymentDetailRepository : GenericRepository<PaymentDetail, PreloadDbContext>, IPaymentDetailRepository
{
    public PaymentDetailRepository(PreloadDbContext context) : base(context)
    {
    }
}
