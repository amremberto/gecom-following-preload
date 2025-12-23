using GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;
using GeCom.Following.Preload.Domain.Spd_Sap.SapPurchaseOrders;

namespace GeCom.Following.Preload.Application.Mappings;

internal static class SapPurchaseOrderMappings
{
    public static SapPurchaseOrderResponse ToResponse(SapPurchaseOrder purchaseOrder)
    {
        SapPurchaseOrderResponse result = new(
            purchaseOrder.Idorden,
            purchaseOrder.Fechadocumento,
            purchaseOrder.Nrodocumento,
            purchaseOrder.Posicion,
            purchaseOrder.Material,
            purchaseOrder.Textobreve,
            purchaseOrder.Codigosociedadfi,
            purchaseOrder.Empresa,
            purchaseOrder.Centro,
            purchaseOrder.Almacen,
            purchaseOrder.Cantidadpedida,
            purchaseOrder.UnidadCp,
            purchaseOrder.Cantidadentregada,
            purchaseOrder.UnidadCe,
            purchaseOrder.Cantidadfacturada,
            purchaseOrder.UnidadCf,
            purchaseOrder.Proveedor,
            purchaseOrder.Condicionpago,
            purchaseOrder.Usuario,
            purchaseOrder.Importeoriginal,
            purchaseOrder.Direccionentrega,
            purchaseOrder.Borrado,
            purchaseOrder.Bloqueado,
            purchaseOrder.EntregaFinal,
            purchaseOrder.Tipo,
            purchaseOrder.Moneda,
            purchaseOrder.Localidad,
            purchaseOrder.Liberada,
            purchaseOrder.Dist,
            purchaseOrder.NetoAnticipo,
            // Campos adicionales no disponibles desde la entidad base
            CantidadFaltaFacturar: null,
            CantidadAFacturar: null,
            CodigoRecepcion: null
        );

        return result;
    }
}
