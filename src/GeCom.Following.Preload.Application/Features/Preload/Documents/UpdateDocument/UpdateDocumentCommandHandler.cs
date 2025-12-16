using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.Domain.Preloads.States;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Interfaces;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.UpdateDocument;

/// <summary>
/// Handler for the UpdateDocumentCommand.
/// </summary>
internal sealed class UpdateDocumentCommandHandler : ICommandHandler<UpdateDocumentCommand, DocumentResponse>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IStateRepository _stateRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDocumentCommandHandler"/> class.
    /// </summary>
    /// <param name="documentRepository">The document repository.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    public UpdateDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IStateRepository stateRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _stateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <inheritdoc />
    public async Task<Result<DocumentResponse>> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verificar si el documento existe (usando GetByIdAsync que incluye tracking)
        Document? document = await _documentRepository.GetByIdAsync(request.DocId, cancellationToken);
        if (document is null)
        {
            return Result.Failure<DocumentResponse>(
                Error.NotFound(
                    "Document.NotFound",
                    $"Document with ID '{request.DocId}' was not found."));
        }

        // Actualizar los campos solo si se proporcionan valores (actualización parcial)
        if (request.ProveedorCuit is not null)
        {
            document.ProveedorCuit = request.ProveedorCuit;
        }

        if (request.SociedadCuit is not null)
        {
            document.SociedadCuit = request.SociedadCuit;
        }

        if (request.TipoDocId.HasValue)
        {
            document.TipoDocId = request.TipoDocId;
        }

        if (request.PuntoDeVenta is not null)
        {
            //Complete the sale point with leading zeros to make it 5 characters long
            document.PuntoDeVenta = request.PuntoDeVenta.PadLeft(5, '0');
        }

        if (request.NumeroComprobante is not null)
        {
            //Complete the receipt number with leading zeros to make it 8 characters long
            document.NumeroComprobante = request.NumeroComprobante.PadLeft(8, '0');
        }

        if (request.FechaEmisionComprobante.HasValue)
        {
            document.FechaEmisionComprobante = request.FechaEmisionComprobante;
        }

        if (request.Moneda is not null)
        {
            document.Moneda = request.Moneda;
        }

        if (request.MontoBruto.HasValue)
        {
            document.MontoBruto = request.MontoBruto;
        }

        if (request.CodigoDeBarras is not null)
        {
            document.CodigoDeBarras = request.CodigoDeBarras;
        }

        if (request.Caecai is not null)
        {
            document.Caecai = request.Caecai;
        }

        if (request.VencimientoCaecai.HasValue)
        {
            document.VencimientoCaecai = request.VencimientoCaecai;
        }

        if (request.NombreSolicitante is not null)
        {
            document.NombreSolicitante = request.NombreSolicitante;
        }

        // Update the user who made the update
        document.UserCreate = request.UserEmail;

        // Update the document in the repository
        Document updatedDocument = await _documentRepository.UpdateAsync(document, cancellationToken);

        // Save changes in the unit of work
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Update DocumentState entity with the new state Precargado, only if the state is PendPrecarga
        // Find the state with code "PendPrecarga"
        State? pendPrecargaState = await _stateRepository.GetByCodeAsync("PendPrecarga", cancellationToken);
        if (pendPrecargaState is null)
        {
            return Result.Failure<DocumentResponse>(
                Error.NotFound(
                    "State.NotFound",
                    "State with code 'PendPrecarga' was not found. Please ensure the state exists in the database."));
        }
        // If the current state is PendPrecarga, update it to Precargado
        if (updatedDocument.EstadoId == pendPrecargaState.EstadoId)
        {
            State? precargadoState = await _stateRepository.GetByCodeAsync("Precargado", cancellationToken);
            if (precargadoState is null)
            {
                return Result.Failure<DocumentResponse>(
                    Error.NotFound(
                        "State.NotFound",
                        "State with code 'Precargado' was not found. Please ensure the state exists in the database."));
            }
            updatedDocument.EstadoId = precargadoState.EstadoId;

            Document updatedDocumentWithPrecargado = await _documentRepository.UpdateAsync(updatedDocument, cancellationToken);

            // Save changes in the unit of work
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Mapear a Response
            DocumentResponse responseWithPrecargado = DocumentMappings.ToResponse(updatedDocumentWithPrecargado);
            return Result.Success(responseWithPrecargado);
        }

        // Mapear a Response
        DocumentResponse response = DocumentMappings.ToResponse(updatedDocument);

        return Result.Success(response);
    }
}
