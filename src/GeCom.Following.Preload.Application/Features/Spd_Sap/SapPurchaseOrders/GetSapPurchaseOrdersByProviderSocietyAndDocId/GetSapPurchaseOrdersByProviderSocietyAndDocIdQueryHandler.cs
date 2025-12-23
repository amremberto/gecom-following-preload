using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.DTOs;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.Domain.Preloads.Societies;
using GeCom.Following.Preload.Domain.Spd_Sap.SapPurchaseOrders;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Spd_Sap.SapPurchaseOrders.GetSapPurchaseOrdersByProviderSocietyAndDocId;

/// <summary>
/// Handler for the GetSapPurchaseOrdersByProviderSocietyAndDocIdQuery.
/// Retrieves SAP purchase orders filtered by provider code, society code, and document number.
/// </summary>
internal sealed class GetSapPurchaseOrdersByProviderSocietyAndDocIdQueryHandler
    : IQueryHandler<GetSapPurchaseOrdersByProviderSocietyAndDocIdQuery, IEnumerable<SapPurchaseOrderResponse>>
{
    private readonly ISapPurchaseOrderRepository _sapPurchaseOrderRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentTypeRepository _documentTypeRepository;
    private readonly ISocietyRepository _societyRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSapPurchaseOrdersByProviderSocietyAndDocIdQueryHandler"/> class.
    /// </summary>
    /// <param name="sapPurchaseOrderRepository">The SAP purchase order repository.</param>
    public GetSapPurchaseOrdersByProviderSocietyAndDocIdQueryHandler(
        ISapPurchaseOrderRepository sapPurchaseOrderRepository,
        IDocumentRepository documentRepository,
        IDocumentTypeRepository documentTypeRepository,
        ISocietyRepository societyRepository)
    {
        _sapPurchaseOrderRepository = sapPurchaseOrderRepository ?? throw new ArgumentNullException(nameof(sapPurchaseOrderRepository));
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _documentTypeRepository = documentTypeRepository ?? throw new ArgumentNullException(nameof(documentTypeRepository));
        _societyRepository = societyRepository ?? throw new ArgumentNullException(nameof(societyRepository));
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<SapPurchaseOrderResponse>>> Handle(
        GetSapPurchaseOrdersByProviderSocietyAndDocIdQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Check if the document exists
        Document? document = await _documentRepository.GetByIdAsync(
            request.DocId,
            cancellationToken);

        if (document is null)
        {
            return Result.Failure<IEnumerable<SapPurchaseOrderResponse>>(
                Error.NotFound(
                    "Document.NotFound",
                    $"Document with ID '{request.DocId}' was not found."));
        }

        if (document.TipoDocId is null)
        {
            return Result.Failure<IEnumerable<SapPurchaseOrderResponse>>(
                Error.NotFound(
                    "DocumentType.NotFound",
                    $"DocumentType for Document with ID '{request.DocId}' was not found."));
        }

        // Get the code of the society associated with the document
        Society? society = await _societyRepository.GetByCuitAsync(
            request.SocietyCuit,
            cancellationToken);

        if (society is null)
        {
            return Result.Failure<IEnumerable<SapPurchaseOrderResponse>>(
                Error.NotFound(
                    "Society.NotFound",
                    $"Society with CUIT '{request.SocietyCuit}' was not found."));
        }

        // Check if the document is a credit or debit note to invoke the appropriate logic
        bool isCreditOrDebitNote = await _documentTypeRepository
            .IsCreditOrDebitNoteAsync(document.TipoDocId, cancellationToken);

        IEnumerable<SapPurchaseOrderResponse> response;

        if (isCreditOrDebitNote)
        {
            // Para notas de crédito/débito, obtener los datos crudos y aplicar la lógica de negocio
            IEnumerable<SapPurchaseOrderCreditDebitNoteDto> rawData = await _sapPurchaseOrderRepository
                .GetCreditDebitNoteDataAsync(
                    request.ProviderCuit,
                    society.Codigo,
                    request.DocId,
                    cancellationToken);

            response = ApplyCreditDebitNoteBusinessLogic(rawData);
        }
        else
        {
            // Para documentos normales, obtener los datos crudos y aplicar la lógica de negocio estándar
            IEnumerable<SapPurchaseOrderCreditDebitNoteDto> rawData = await _sapPurchaseOrderRepository
                .GetStandardPurchaseOrderDataAsync(
                    request.ProviderCuit,
                    society.Codigo,
                    request.DocId,
                    cancellationToken);

            response = ApplyStandardBusinessLogic(rawData);
        }

        return Result.Success(response);
    }

    /// <summary>
    /// Aplica la lógica de negocio específica para notas de crédito/débito.
    /// Esta lógica incluye:
    /// - Duplicar registros cuando hay CodigoRecepcion y CantidadAFacturar
    /// - Calcular campos derivados (CantidadFaltaFacturar, ImporteTotal)
    /// - Ordenar los resultados
    /// </summary>
    private static IEnumerable<SapPurchaseOrderResponse> ApplyCreditDebitNoteBusinessLogic(
        IEnumerable<SapPurchaseOrderCreditDebitNoteDto> rawData)
    {
        List<SapPurchaseOrderCreditDebitNoteResponse> result = [];
        HashSet<string> processedOrderNumbers = [];

        foreach (SapPurchaseOrderCreditDebitNoteDto dto in rawData)
        {
            decimal cantidadFaltaFacturar = dto.CantidadRecepcionada - dto.CantidadFacturada;
            decimal? importeTotal = dto.CantidadAFacturar.HasValue
                ? dto.CantidadAFacturar * dto.ImporteOriginal
                : null;

            // Si tiene CodigoRecepcion y CantidadAFacturar, y aún no se procesó esta OC,
            // agregar una entrada adicional con esos campos en null/vacío
            bool hasReceptionCode = !string.IsNullOrWhiteSpace(dto.CodigoRecepcion);
            bool hasQuantityToInvoice = dto.CantidadAFacturar.HasValue;
            bool isFirstTimeForThisOrder = !processedOrderNumbers.Contains(dto.NumeroDocumento);

            if (hasReceptionCode && hasQuantityToInvoice && isFirstTimeForThisOrder)
            {
                // Agregar entrada adicional sin CodigoRecepcion y CantidadAFacturar
                result.Add(new SapPurchaseOrderCreditDebitNoteResponse(
                    dto.OrdenCompraId,
                    dto.NumeroDocumento,
                    dto.Posicion,
                    dto.DescripcionProducto,
                    dto.UnidadMedida,
                    dto.FechaEmisionOC,
                    dto.CantidadPedida,
                    dto.CantidadRecepcionada,
                    dto.CantidadFacturada,
                    cantidadFaltaFacturar,
                    null, // CantidadAFacturar = null
                    string.Empty, // CodigoRecepcion = vacío
                    dto.ImporteOriginal,
                    null, // ImporteTotal = null cuando CantidadAFacturar es null
                    dto.CodigoSociedadFI,
                    dto.ProveedorSAP,
                    dto.Contacto,
                    dto.NetoAnticipo));

                processedOrderNumbers.Add(dto.NumeroDocumento);
            }

            // Agregar la entrada normal con todos los datos
            result.Add(new SapPurchaseOrderCreditDebitNoteResponse(
                dto.OrdenCompraId,
                dto.NumeroDocumento,
                dto.Posicion,
                dto.DescripcionProducto,
                dto.UnidadMedida,
                dto.FechaEmisionOC,
                dto.CantidadPedida,
                dto.CantidadRecepcionada,
                dto.CantidadFacturada,
                cantidadFaltaFacturar,
                dto.CantidadAFacturar,
                dto.CodigoRecepcion ?? string.Empty,
                dto.ImporteOriginal,
                importeTotal,
                dto.CodigoSociedadFI,
                dto.ProveedorSAP,
                dto.Contacto,
                dto.NetoAnticipo));
        }

        // Ordenar: primero por CantidadAFacturar descendente, luego por NumeroDocumento descendente, luego por Posicion ascendente
        return result
            .OrderByDescending(r => r.CantidadAFacturar)
            .ThenByDescending(r => r.NumeroDocumento)
            .ThenBy(r => r.Posicion)
            .Select(MapToSapPurchaseOrderResponse);
    }

    /// <summary>
    /// Mapea el DTO de respuesta extendido al DTO de respuesta estándar.
    /// </summary>
    private static SapPurchaseOrderResponse MapToSapPurchaseOrderResponse(
        SapPurchaseOrderCreditDebitNoteResponse extended)
    {
        return new SapPurchaseOrderResponse(
            extended.OrdenCompraId,
            extended.FechaEmisionOC,
            extended.NumeroDocumento,
            extended.Posicion,
            null, // Material - no disponible en el DTO extendido
            extended.DescripcionProducto,
            extended.CodigoSociedadFI,
            null, // Empresa - no disponible
            null, // Centro - no disponible
            null, // Almacen - no disponible
            extended.CantidadPedida,
            extended.UnidadMedida,
            extended.CantidadRecepcionada,
            null, // UnidadCe - no disponible
            extended.CantidadFacturada,
            null, // UnidadCf - no disponible
            extended.ProveedorSAP,
            null, // Condicionpago - no disponible
            extended.Contacto,
            extended.ImporteOriginal,
            null, // Direccionentrega - no disponible
            null, // Borrado - no disponible
            null, // Bloqueado - no disponible
            null, // EntregaFinal - no disponible
            null, // Tipo - no disponible
            null, // Moneda - no disponible
            null, // Localidad - no disponible
            0, // Liberada - no disponible, usar valor por defecto
            null, // Dist - no disponible
            extended.ImporteNetoAnticipo,
            // Campos adicionales calculados
            extended.CantidadFaltaFacturar,
            extended.CantidadAFacturar,
            extended.CodigoRecepcion);
    }

    /// <summary>
    /// Aplica la lógica de negocio específica para documentos estándar (no notas de crédito/débito).
    /// Esta lógica incluye:
    /// - Duplicar registros cuando hay CodigoRecepcion (verificando que no exista ya una entrada con CodigoRecepcion vacío)
    /// - Calcular campos derivados (CantidadFaltaFacturar, ImporteTotal)
    /// - Ordenar los resultados
    /// </summary>
    private static IEnumerable<SapPurchaseOrderResponse> ApplyStandardBusinessLogic(
        IEnumerable<SapPurchaseOrderCreditDebitNoteDto> rawData)
    {
        List<SapPurchaseOrderCreditDebitNoteResponse> result = [];

        foreach (SapPurchaseOrderCreditDebitNoteDto dto in rawData)
        {
            decimal cantidadFaltaFacturar = dto.CantidadRecepcionada - dto.CantidadFacturada;
            decimal? importeTotal = dto.CantidadAFacturar.HasValue
                ? dto.CantidadAFacturar * dto.ImporteOriginal
                : null;

            bool hasReceptionCode = !string.IsNullOrWhiteSpace(dto.CodigoRecepcion);

            // Si tiene CodigoRecepcion, verificar si ya existe una entrada con ese NumeroDocumento y CodigoRecepcion vacío
            // Si no existe, agregar una entrada adicional con CodigoRecepcion vacío y CantidadAFacturar null
            // Nota: ImporteTotal se calcula usando cantAFacturar del registro original (aunque CantidadAFacturar se establece en null)
            if (hasReceptionCode)
            {
                bool existsEntryWithEmptyReceptionCode = result
                    .Any(r => r.NumeroDocumento == dto.NumeroDocumento 
                           && string.IsNullOrEmpty(r.CodigoRecepcion));

                if (!existsEntryWithEmptyReceptionCode)
                {
                    // Agregar entrada adicional sin CodigoRecepcion y CantidadAFacturar null
                    // ImporteTotal se calcula usando cantAFacturar del DTO original (comportamiento del código antiguo)
                    result.Add(new SapPurchaseOrderCreditDebitNoteResponse(
                        dto.OrdenCompraId,
                        dto.NumeroDocumento,
                        dto.Posicion,
                        dto.DescripcionProducto,
                        dto.UnidadMedida,
                        dto.FechaEmisionOC,
                        dto.CantidadPedida,
                        dto.CantidadRecepcionada,
                        dto.CantidadFacturada,
                        cantidadFaltaFacturar,
                        null, // CantidadAFacturar = null
                        string.Empty, // CodigoRecepcion = vacío
                        dto.ImporteOriginal,
                        importeTotal, // Usar el importeTotal calculado con cantAFacturar del registro original
                        dto.CodigoSociedadFI,
                        dto.ProveedorSAP,
                        dto.Contacto,
                        dto.NetoAnticipo));
                }
            }

            // Agregar la entrada normal con todos los datos
            result.Add(new SapPurchaseOrderCreditDebitNoteResponse(
                dto.OrdenCompraId,
                dto.NumeroDocumento,
                dto.Posicion,
                dto.DescripcionProducto,
                dto.UnidadMedida,
                dto.FechaEmisionOC,
                dto.CantidadPedida,
                dto.CantidadRecepcionada,
                dto.CantidadFacturada,
                cantidadFaltaFacturar,
                dto.CantidadAFacturar,
                dto.CodigoRecepcion ?? string.Empty,
                dto.ImporteOriginal,
                importeTotal,
                dto.CodigoSociedadFI,
                dto.ProveedorSAP,
                dto.Contacto,
                dto.NetoAnticipo));
        }

        // Ordenar: primero por CantidadAFacturar descendente, luego por NumeroDocumento descendente, luego por Posicion ascendente
        return result
            .OrderByDescending(r => r.CantidadAFacturar)
            .ThenByDescending(r => r.NumeroDocumento)
            .ThenBy(r => r.Posicion)
            .Select(MapToSapPurchaseOrderResponse);
    }
}

