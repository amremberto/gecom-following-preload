namespace GeCom.Following.Preload.Application.DTOs;

/// <summary>
/// DTO para el resultado de la consulta SQL de órdenes de compra con notas de crédito/débito.
/// Contiene los datos crudos obtenidos de la base de datos antes de aplicar la lógica de negocio.
/// </summary>
public sealed record SapPurchaseOrderCreditDebitNoteDto
{
    public long OrdenCompraId { get; init; }
    public string NumeroDocumento { get; init; } = null!;
    public int Posicion { get; init; }
    public string? DescripcionProducto { get; init; }
    public string? UnidadMedida { get; init; }
    public DateTime? FechaEmisionOC { get; init; }
    public decimal CantidadPedida { get; init; }
    public decimal CantidadRecepcionada { get; init; }
    public decimal CantidadFacturada { get; init; }
    public decimal? CantidadAFacturar { get; init; }
    public string? CodigoRecepcion { get; init; }
    public decimal ImporteOriginal { get; init; }
    public string? CodigoSociedadFI { get; init; }
    public string ProveedorSAP { get; init; } = null!;
    public string? Contacto { get; init; }
    public decimal? NetoAnticipo { get; init; }
}

