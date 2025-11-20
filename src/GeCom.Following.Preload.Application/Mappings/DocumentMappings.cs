using GeCom.Following.Preload.Contracts.Preload.Attachments;
using GeCom.Following.Preload.Contracts.Preload.Documents;
using GeCom.Following.Preload.Contracts.Preload.Notes;
using GeCom.Following.Preload.Contracts.Preload.PurchaseOrders;
using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.Domain.Preloads.Notes;

namespace GeCom.Following.Preload.Application.Mappings;

internal static class DocumentMappings
{
    public static DocumentResponse ToResponse(Document document)
    {
        ICollection<PurchaseOrderResponse> purchaseOrders =
            [.. document.PurchaseOrders.Select(PurchaseOrderMappings.ToResponse)];

        ICollection<NoteResponse> notes =
            [.. document.Notes.Select(NoteMappings.ToResponse)];

        ICollection<AttachmentResponse> attachments =
            [.. document.Attachments.Select(AttachmentMappings.ToResponse)];

        DocumentResponse result = new(
            document.DocId,
            document.ProveedorCuit,
            document.Provider?.RazonSocial,
            document.SociedadCuit,
            document.Society?.Descripcion,
            document.TipoDocId,
            document.DocumentType?.Descripcion,
            document.PuntoDeVenta,
            document.NumeroComprobante,
            document.FechaEmisionComprobante,
            document.Moneda,
            document.MontoBruto,
            document.CodigoDeBarras,
            document.Caecai,
            document.VencimientoCaecai,
            document.EstadoId,
            document.State?.Descripcion,
            document.FechaCreacion,
            document.FechaBaja,
            document.IdDocument,
            document.NombreSolicitante,
            document.IdDetalleDePago,
            document.IdMetodoDePago,
            document.FechaPago,
            document.UserCreate,
            purchaseOrders,
            notes,
            attachments
        );

        return result;
    }
}

internal static class PurchaseOrderMappings
{
    public static PurchaseOrderResponse ToResponse(Domain.Preloads.PurchaseOrders.PurchaseOrder purchaseOrder)
    {
        PurchaseOrderResponse result = new(
            purchaseOrder.Ocid,
            purchaseOrder.CodigoRecepcion,
            purchaseOrder.CantidadAfacturar,
            purchaseOrder.DocId,
            purchaseOrder.OrdenCompraId,
            purchaseOrder.FechaCreacion,
            purchaseOrder.FechaBaja,
            purchaseOrder.NroOc,
            purchaseOrder.PosicionOc,
            purchaseOrder.CodigoSociedadFi,
            purchaseOrder.ProveedorSap
        );

        return result;
    }
}

internal static class NoteMappings
{
    public static NoteResponse ToResponse(Note note)
    {
        NoteResponse result = new(
            note.NotaId,
            note.Descripcion,
            note.UsuarioCreacion,
            note.FechaCreacion,
            note.DocId
        );

        return result;
    }
}

