using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Currencies;
using GeCom.Following.Preload.Domain.Preloads.Currencies;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Currencies.CreateCurrency;

/// <summary>
/// Handler for the CreateCurrencyCommand.
/// </summary>
internal sealed class CreateCurrencyCommandHandler : ICommandHandler<CreateCurrencyCommand, CurrencyResponse>
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCurrencyCommandHandler"/> class.
    /// </summary>
    /// <param name="currencyRepository">The currency repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public CreateCurrencyCommandHandler(ICurrencyRepository currencyRepository, IUnitOfWork unitOfWork)
    {
        _currencyRepository = currencyRepository ?? throw new ArgumentNullException(nameof(currencyRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<CurrencyResponse>> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si ya existe una Currency con el mismo código
        Currency? existingByCodigo = await _currencyRepository.GetByCodeAsync(request.Codigo, cancellationToken);
        if (existingByCodigo is not null)
        {
            return Result.Failure<CurrencyResponse>(
                Error.Conflict(
                    "Currency.Conflict",
                    $"A currency with code '{request.Codigo}' already exists."));
        }

        // Crear la nueva Currency
        Currency currency = new()
        {
            Codigo = request.Codigo,
            Descripcion = request.Descripcion,
            CodigoAfip = request.CodigoAfip
        };

        // Agregar al repositorio
        Currency addedCurrency = await _currencyRepository.AddAsync(currency, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        CurrencyResponse response = CurrencyMappings.ToResponse(addedCurrency);

        return Result.Success(response);
    }
}

