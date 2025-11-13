using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.Domain.Preloads.States;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.CreateDocument;

/// <summary>
/// Handler for the CreateDocumentCommand.
/// </summary>
internal sealed class CreateDocumentCommandHandler : ICommandHandler<CreateDocumentCommand, DocumentResponse>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IStateRepository _stateRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDocumentCommandHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="stateRepository">The state repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public CreateDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IStateRepository stateRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _stateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<DocumentResponse>> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Buscar el estado con código "Precargado"
        State? precargadoState = await _stateRepository.GetByCodeAsync("Precargado", cancellationToken);
        if (precargadoState is null)
        {
            return Result.Failure<DocumentResponse>(
                Error.NotFound(
                    "State.NotFound",
                    "State with code 'Precargado' was not found. Please ensure the state exists in the database."));
        }

        // Crear la nueva entidad Document
        Document document = new()
        {
            ProveedorCuit = request.ProveedorCuit,
            SociedadCuit = request.SociedadCuit,
            TipoDocId = request.TipoDocId,
            PuntoDeVenta = request.PuntoDeVenta,
            NumeroComprobante = request.NumeroComprobante,
            FechaEmisionComprobante = request.FechaEmisionComprobante,
            Moneda = request.Moneda,
            MontoBruto = request.MontoBruto,
            Caecai = request.Caecai,
            VencimientoCaecai = request.VencimientoCaecai,
            EstadoId = precargadoState.EstadoId,
            NombreSolicitante = request.NombreSolicitante,
            FechaCreacion = DateTime.UtcNow
        };

        // Agregar al repositorio
        Document addedDocument = await _documentRepository.AddAsync(document, cancellationToken);

        // Guardar cambios
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a Response
        DocumentResponse response = DocumentMappings.ToResponse(addedDocument);

        return Result.Success(response);
    }
}

