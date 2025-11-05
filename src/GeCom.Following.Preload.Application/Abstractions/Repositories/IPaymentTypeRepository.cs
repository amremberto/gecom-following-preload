using GeCom.Following.Preload.Domain.Preloads.PaymentTypes;
using GeCom.Following.Preload.SharedKernel.Interfaces;

namespace GeCom.Following.Preload.Application.Abstractions.Repositories;

/// <summary>
/// Repository interface for PaymentType entities.
/// </summary>
public interface IPaymentTypeRepository : IRepository<PaymentType>
{
    /// <summary>
    /// Gets a payment type by its description.
    /// </summary>
    /// <param name="descripcion">Payment type description.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The payment type or null.</returns>
    Task<PaymentType?> GetByDescripcionAsync(string descripcion, CancellationToken cancellationToken = default);
}
