using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.Domain.Preloads.Societies;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.CreateSociety;

/// <summary>
/// Handler for the CreateSocietyCommand.
/// </summary>
internal sealed class CreateSocietyCommandHandler : ICommandHandler<CreateSocietyCommand, SocietyResponse>
{
    private readonly ISocietyRepository _societyRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSocietyCommandHandler"/> class.
    /// </summary>
    /// <param name="societyRepository">The society repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public CreateSocietyCommandHandler(ISocietyRepository societyRepository, IUnitOfWork unitOfWork)
    {
        _societyRepository = societyRepository ?? throw new ArgumentNullException(nameof(societyRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<SocietyResponse>> Handle(CreateSocietyCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si ya existe una Society con el mismo código
        Society? existingByCodigo = await _societyRepository.GetByCodigoAsync(request.Codigo, cancellationToken);
        if (existingByCodigo is not null)
        {
            return Result.Failure<SocietyResponse>(
                Error.Conflict(
                    "Society.Conflict",
                    $"A society with code '{request.Codigo}' already exists."));
        }

        // Verificar si ya existe una Society con el mismo CUIT
        Society? existingByCuit = await _societyRepository.GetByCuitAsync(request.Cuit, cancellationToken);
        if (existingByCuit is not null)
        {
            return Result.Failure<SocietyResponse>(
                Error.Conflict(
                    "Society.Conflict",
                    $"A society with CUIT '{request.Cuit}' already exists."));
        }

        // Crear la nueva Society
        Society society = new()
        {
            Codigo = request.Codigo,
            Descripcion = request.Descripcion,
            Cuit = request.Cuit,
            FechaCreacion = DateTime.UtcNow,
            EsPrecarga = request.EsPrecarga
        };

        // Agregar al repositorio
        Society addedSociety = await _societyRepository.AddAsync(society, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        SocietyResponse response = SocietyMappings.ToResponse(addedSociety);

        return Result.Success(response);
    }
}

