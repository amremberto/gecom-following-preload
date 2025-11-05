using GeCom.Following.Preload.Domain.Preloads.ActionsRegisters;
using GeCom.Following.Preload.Domain.Preloads.Attachments;
using GeCom.Following.Preload.Domain.Preloads.Currencies;
using GeCom.Following.Preload.Domain.Preloads.DocumentStates;
using GeCom.Following.Preload.Domain.Preloads.DocumentTypes;
using GeCom.Following.Preload.Domain.Preloads.Notes;
using GeCom.Following.Preload.Domain.Preloads.PaymentDetails;
using GeCom.Following.Preload.Domain.Preloads.Providers;
using GeCom.Following.Preload.Domain.Preloads.PurchaseOrders;
using GeCom.Following.Preload.Domain.Preloads.Societies;
using GeCom.Following.Preload.Domain.Preloads.States;
using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Preloads.Documents;

/// <summary>
/// Represents a document entity in the preload system.
/// </summary>
/// <remarks>
/// This entity contains information about documents including provider details, 
/// document type, amounts, and related entities like attachments and purchase orders.
/// </remarks>
public partial class Document : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique document identifier.
    /// </summary>
    public int DocId { get; set; }

    /// <summary>
    /// Gets or sets the provider's CUIT (tax identification number).
    /// </summary>
    public string? ProveedorCuit { get; set; }

    /// <summary>
    /// Gets or sets the society's CUIT (tax identification number).
    /// </summary>
    public string? SociedadCuit { get; set; }

    /// <summary>
    /// Gets or sets the document type identifier.
    /// </summary>
    public int? TipoDocId { get; set; }

    /// <summary>
    /// Gets or sets the point of sale identifier.
    /// </summary>
    public string? PuntoDeVenta { get; set; }

    /// <summary>
    /// Gets or sets the document number.
    /// </summary>
    public string? NumeroComprobante { get; set; }

    /// <summary>
    /// Gets or sets the document emission date.
    /// </summary>
    public DateOnly? FechaEmisionComprobante { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string? Moneda { get; set; }

    /// <summary>
    /// Gets or sets the gross amount of the document.
    /// </summary>
    public decimal? MontoBruto { get; set; }

    /// <summary>
    /// Gets or sets the barcode of the document.
    /// </summary>
    public string? CodigoDeBarras { get; set; }

    /// <summary>
    /// Gets or sets the CAECAI (electronic authorization code).
    /// </summary>
    public string? Caecai { get; set; }

    /// <summary>
    /// Gets or sets the CAECAI expiration date.
    /// </summary>
    public DateOnly? VencimientoCaecai { get; set; }

    /// <summary>
    /// Gets or sets the state identifier.
    /// </summary>
    public int? EstadoId { get; set; }

    /// <summary>
    /// Gets or sets the document creation date.
    /// </summary>
    public DateTime? FechaCreacion { get; set; }

    /// <summary>
    /// Gets or sets the document deletion date.
    /// </summary>
    public DateTime? FechaBaja { get; set; }

    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public int? IdDocument { get; set; }

    /// <summary>
    /// Gets or sets the name of the requester.
    /// </summary>
    public string? NombreSolicitante { get; set; }

    /// <summary>
    /// Gets or sets the payment detail identifier.
    /// </summary>
    public int? IdDetalleDePago { get; set; }

    /// <summary>
    /// Gets or sets the payment method identifier.
    /// </summary>
    public int? IdMetodoDePago { get; set; }

    /// <summary>
    /// Gets or sets the payment date.
    /// </summary>
    public DateTime? FechaPago { get; set; }

    /// <summary>
    /// Gets or sets the user who created the document.
    /// </summary>
    public string? UserCreate { get; set; }

    /// <summary>
    /// Gets or sets the collection of attachments associated with this document.
    /// </summary>
    public virtual ICollection<Attachment> Attachments { get; set; } = [];

    /// <summary>
    /// Gets or sets the state of this document.
    /// </summary>
    public virtual State? State { get; set; }

    /// <summary>
    /// Gets or sets the collection of document states associated with this document.
    /// </summary>
    public virtual ICollection<DocumentState> DocumentStates { get; set; } = [];

    /// <summary>
    /// Gets or sets the payment details for this document.
    /// </summary>
    public virtual PaymentDetail? PaymentDetail { get; set; }

    /// <summary>
    /// Gets or sets the currency used for this document.
    /// </summary>
    public virtual Currency? Currency { get; set; }

    /// <summary>
    /// Gets or sets the collection of notes associated with this document.
    /// </summary>
    public virtual ICollection<Note> Notes { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of purchase orders associated with this document.
    /// </summary>
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = [];

    /// <summary>
    /// Gets or sets the provider associated with this document.
    /// </summary>
    public virtual Provider? Provider { get; set; }

    /// <summary>
    /// Gets or sets the collection of action registers associated with this document.
    /// </summary>
    public virtual ICollection<ActionsRegister> ActionsRegisters { get; set; } = [];

    /// <summary>
    /// Gets or sets the society associated with this document.
    /// </summary>
    public virtual Society? Society { get; set; }

    /// <summary>
    /// Gets or sets the document type of this document.
    /// </summary>
    public virtual DocumentType? DocumentType { get; set; }
}
