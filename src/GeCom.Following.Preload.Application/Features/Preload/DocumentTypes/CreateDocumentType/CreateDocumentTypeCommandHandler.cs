using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.DocumentTypes;
using GeCom.Following.Preload.Domain.Preloads.DocumentTypes;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.DocumentTypes.CreateDocumentType;

/// <summary>
/// Handler for the CreateDocumentTypeCommand.
/// </summary>
internal sealed class CreateDocumentTypeCommandHandler : ICommandHandler<CreateDocumentTypeCommand, DocumentTypeResponse>
{
    private readonly IDocumentTypeRepository _documentTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDocumentTypeCommandHandler"/> class.
    /// </summary>
    /// <param name="documentTypeRepository">The document type repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public CreateDocumentTypeCommandHandler(IDocumentTypeRepository documentTypeRepository, IUnitOfWork unitOfWork)
    {
        _documentTypeRepository = documentTypeRepository ?? throw new ArgumentNullException(nameof(documentTypeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<DocumentTypeResponse>> Handle(CreateDocumentTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si ya existe un DocumentType con el mismo código
        DocumentType? existingByCode = await _documentTypeRepository.GetByCodeAsync(request.Codigo, cancellationToken);
        if (existingByCode is not null)
        {
            return Result.Failure<DocumentTypeResponse>(
                Error.Conflict(
                    "DocumentType.Conflict",
                    $"A document type with code '{request.Codigo}' already exists."));
        }

        // Crear la nueva entidad DocumentType
        DocumentType documentType = new()
        {
            Descripcion = request.Descripcion,
            Letra = request.Letra,
            Codigo = request.Codigo,
            DescripcionLarga = request.DescripcionLarga,
            IsFec = request.IsFec,
            FechaCreacion = DateTime.UtcNow
        };

        // Agregar al repositorio
        DocumentType addedDocumentType = await _documentTypeRepository.AddAsync(documentType, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        DocumentTypeResponse response = DocumentTypeMappings.ToResponse(addedDocumentType);

        return Result.Success(response);
    }
}

