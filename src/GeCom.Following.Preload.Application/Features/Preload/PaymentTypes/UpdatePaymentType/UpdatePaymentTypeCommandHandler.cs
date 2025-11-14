using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.PaymentTypes;
using GeCom.Following.Preload.Domain.Preloads.PaymentTypes;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentTypes.UpdatePaymentType;

/// <summary>
/// Handler for the UpdatePaymentTypeCommand.
/// </summary>
internal sealed class UpdatePaymentTypeCommandHandler : ICommandHandler<UpdatePaymentTypeCommand, PaymentTypeResponse>
{
    private readonly IPaymentTypeRepository _paymentTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePaymentTypeCommandHandler"/> class.
    /// </summary>
    /// <param name="paymentTypeRepository">The payment type repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public UpdatePaymentTypeCommandHandler(
        IPaymentTypeRepository paymentTypeRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentTypeRepository = paymentTypeRepository ?? throw new ArgumentNullException(nameof(paymentTypeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<PaymentTypeResponse>> Handle(UpdatePaymentTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si el PaymentType existe
        PaymentType? paymentType = await _paymentTypeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (paymentType is null)
        {
            return Result.Failure<PaymentTypeResponse>(
                Error.NotFound(
                    "PaymentType.NotFound",
                    $"Payment type with ID '{request.Id}' was not found."));
        }

        // Verificar si ya existe otro PaymentType con la misma descripción (excluyendo el actual)
        PaymentType? existingByDescripcion = await _paymentTypeRepository.GetByDescripcionAsync(request.Descripcion, cancellationToken);
        if (existingByDescripcion is not null && existingByDescripcion.Id != request.Id)
        {
            return Result.Failure<PaymentTypeResponse>(
                Error.Conflict(
                    "PaymentType.Conflict",
                    $"A payment type with description '{request.Descripcion}' already exists."));
        }

        // Actualizar los campos
        paymentType.Descripcion = request.Descripcion;

        // Actualizar en el repositorio
        PaymentType updatedPaymentType = await _paymentTypeRepository.UpdateAsync(paymentType, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        PaymentTypeResponse response = PaymentTypeMappings.ToResponse(updatedPaymentType);

        return Result.Success(response);
    }
}

