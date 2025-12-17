namespace GeCom.Following.Preload.Contracts.Spd_Sap.SapPurchaseOrders;

/// <summary>
/// Response DTO for SapPurchaseOrder.
/// </summary>
public sealed record SapPurchaseOrderResponse(
    long Idorden,
    DateTime? Fechadocumento,
    string Nrodocumento,
    int Posicion,
    string? Material,
    string? Textobreve,
    string? Codigosociedadfi,
    string? Empresa,
    string? Centro,
    string? Almacen,
    decimal Cantidadpedida,
    string? UnidadCp,
    decimal Cantidadentregada,
    string? UnidadCe,
    decimal Cantidadfacturada,
    string? UnidadCf,
    string Proveedor,
    string? Condicionpago,
    string? Usuario,
    decimal Importeoriginal,
    string? Direccionentrega,
    bool? Borrado,
    bool? Bloqueado,
    bool? EntregaFinal,
    string? Tipo,
    string? Moneda,
    string? Localidad,
    int Liberada,
    string? Dist,
    decimal? NetoAnticipo
);
