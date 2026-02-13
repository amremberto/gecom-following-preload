using GeCom.Following.Preload.Domain.Preloads.PaymentDetails;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for PaymentDetail entities.
/// </summary>
public interface IPaymentDetailRepository : IRepository<PaymentDetail>
{
}
