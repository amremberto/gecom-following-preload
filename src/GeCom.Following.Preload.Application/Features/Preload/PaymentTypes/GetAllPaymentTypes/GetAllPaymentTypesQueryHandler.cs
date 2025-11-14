using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.PaymentTypes;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.GetAllPaymentTypes;

/// <summary>
/// Handler for the GetAllPaymentTypesQuery.
/// </summary>
internal sealed class GetAllPaymentTypesQueryHandler : IQueryHandler<GetAllPaymentTypesQuery, IEnumerable<PaymentTypeResponse>>
{
    private readonly IPaymentTypeRepository _paymentTypeRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllPaymentTypesQueryHandler"/> class.
    /// </summary>
    /// <param name="paymentTypeRepository">The payment type repository.</param>
    public GetAllPaymentTypesQueryHandler(IPaymentTypeRepository paymentTypeRepository)
    {
        _paymentTypeRepository = paymentTypeRepository ?? throw new ArgumentNullException(nameof(paymentTypeRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<PaymentTypeResponse>>> Handle(GetAllPaymentTypesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        IEnumerable<Domain.Preloads.PaymentTypes.PaymentType> paymentTypes = await _paymentTypeRepository.GetAllAsync(cancellationToken);

        IEnumerable<PaymentTypeResponse> response = paymentTypes
            .Select(PaymentTypeMappings.ToResponse);

        return Result.Success(response);
    }
}

