using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.Domain.Preloads.Societies;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.UpdateSociety;

/// <summary>
/// Handler for the UpdateSocietyCommand.
/// </summary>
internal sealed class UpdateSocietyCommandHandler : ICommandHandler<UpdateSocietyCommand, SocietyResponse>
{
    private readonly ISocietyRepository _societyRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSocietyCommandHandler"/> class.
    /// </summary>
    /// <param name="societyRepository">The society repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public UpdateSocietyCommandHandler(ISocietyRepository societyRepository, IUnitOfWork unitOfWork)
    {
        _societyRepository = societyRepository ?? throw new ArgumentNullException(nameof(societyRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<SocietyResponse>> Handle(UpdateSocietyCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si la Society existe
        Society? society = await _societyRepository.GetByIdAsync(request.Id, cancellationToken);
        if (society is null)
        {
            return Result.Failure<SocietyResponse>(
                Error.NotFound(
                    "Society.NotFound",
                    $"Society with ID '{request.Id}' was not found."));
        }

        // Verificar si ya existe otra Society con el mismo código (excluyendo la actual)
        Society? existingByCodigo = await _societyRepository.GetByCodigoAsync(request.Codigo, cancellationToken);
        if (existingByCodigo is not null && existingByCodigo.SocId != request.Id)
        {
            return Result.Failure<SocietyResponse>(
                Error.Conflict(
                    "Society.Conflict",
                    $"A society with code '{request.Codigo}' already exists."));
        }

        // Verificar si ya existe otra Society con el mismo CUIT (excluyendo la actual)
        Society? existingByCuit = await _societyRepository.GetByCuitAsync(request.Cuit, cancellationToken);
        if (existingByCuit is not null && existingByCuit.SocId != request.Id)
        {
            return Result.Failure<SocietyResponse>(
                Error.Conflict(
                    "Society.Conflict",
                    $"A society with CUIT '{request.Cuit}' already exists."));
        }

        // Actualizar los campos
        society.Codigo = request.Codigo;
        society.Descripcion = request.Descripcion;
        society.Cuit = request.Cuit;
        society.EsPrecarga = request.EsPrecarga;

        // Actualizar en el repositorio
        Society updatedSociety = await _societyRepository.UpdateAsync(society, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        SocietyResponse response = SocietyMappings.ToResponse(updatedSociety);

        return Result.Success(response);
    }
}

