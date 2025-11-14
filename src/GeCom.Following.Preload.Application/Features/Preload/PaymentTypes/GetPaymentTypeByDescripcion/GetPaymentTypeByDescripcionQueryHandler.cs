using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.PaymentTypes;
using GeCom.Following.Preload.Domain.Preloads.PaymentTypes;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.GetPaymentTypeByDescripcion;

/// <summary>
/// Handler for the GetPaymentTypeByDescripcionQuery.
/// </summary>
internal sealed class GetPaymentTypeByDescripcionQueryHandler : IQueryHandler<GetPaymentTypeByDescripcionQuery, PaymentTypeResponse>
{
    private readonly IPaymentTypeRepository _paymentTypeRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPaymentTypeByDescripcionQueryHandler"/> class.
    /// </summary>
    /// <param name="paymentTypeRepository">The payment type repository.</param>
    public GetPaymentTypeByDescripcionQueryHandler(IPaymentTypeRepository paymentTypeRepository)
    {
        _paymentTypeRepository = paymentTypeRepository ?? throw new ArgumentNullException(nameof(paymentTypeRepository));
    }

    /// <inheritdoc />
    public async Task<Result<PaymentTypeResponse>> Handle(GetPaymentTypeByDescripcionQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        PaymentType? paymentType = await _paymentTypeRepository.GetByDescripcionAsync(request.Descripcion, cancellationToken);

        if (paymentType is null)
        {
            return Result.Failure<PaymentTypeResponse>(
                Error.NotFound(
                    "PaymentType.NotFound",
                    $"Payment type with description '{request.Descripcion}' was not found."));
        }

        PaymentTypeResponse response = PaymentTypeMappings.ToResponse(paymentType);

        return Result.Success(response);
    }
}

