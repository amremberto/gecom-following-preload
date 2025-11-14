using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.PaymentTypes;
using GeCom.Following.Preload.Domain.Preloads.PaymentTypes;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.CreatePaymentType;

/// <summary>
/// Handler for the CreatePaymentTypeCommand.
/// </summary>
internal sealed class CreatePaymentTypeCommandHandler : ICommandHandler<CreatePaymentTypeCommand, PaymentTypeResponse>
{
    private readonly IPaymentTypeRepository _paymentTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePaymentTypeCommandHandler"/> class.
    /// </summary>
    /// <param name="paymentTypeRepository">The payment type repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public CreatePaymentTypeCommandHandler(
        IPaymentTypeRepository paymentTypeRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentTypeRepository = paymentTypeRepository ?? throw new ArgumentNullException(nameof(paymentTypeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<PaymentTypeResponse>> Handle(CreatePaymentTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si ya existe un PaymentType con la misma descripción
        PaymentType? existing = await _paymentTypeRepository.GetByDescripcionAsync(request.Descripcion, cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<PaymentTypeResponse>(
                Error.Conflict(
                    "PaymentType.Conflict",
                    $"A payment type with description '{request.Descripcion}' already exists."));
        }

        // Crear la nueva entidad PaymentType
        PaymentType paymentType = new()
        {
            Descripcion = request.Descripcion
        };

        // Agregar al repositorio
        PaymentType addedPaymentType = await _paymentTypeRepository.AddAsync(paymentType, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        PaymentTypeResponse response = PaymentTypeMappings.ToResponse(addedPaymentType);

        return Result.Success(response);
    }
}

