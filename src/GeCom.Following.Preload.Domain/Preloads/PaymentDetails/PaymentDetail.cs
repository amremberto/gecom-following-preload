using GeCom.Following.Preload.Domain.Preloads.Documents;
using GeCom.Following.Preload.Domain.Preloads.PaymentTypes;
using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Preloads.PaymentDetails;

/// <summary>
/// Represents a payment detail entity in the preload system.
/// </summary>
/// <remarks>
/// This entity contains information about payment details including payment types,
/// check numbers, banks, amounts, and associated documents.
/// </remarks>
public partial class PaymentDetail : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique payment detail identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the payment type identifier.
    /// </summary>
    public int IdTipoDePago { get; set; }

    /// <summary>
    /// Gets or sets the check number.
    /// </summary>
    public string NroCheque { get; set; } = null!;

    /// <summary>
    /// Gets or sets the bank name.
    /// </summary>
    public string Banco { get; set; } = null!;

    /// <summary>
    /// Gets or sets the payment due date.
    /// </summary>
    public DateOnly Vencimiento { get; set; }

    /// <summary>
    /// Gets or sets the received amount.
    /// </summary>
    public decimal ImporteRecibido { get; set; }

    /// <summary>
    /// Gets or sets the registration date.
    /// </summary>
    public DateOnly FechaAlta { get; set; }

    /// <summary>
    /// Gets or sets the PDF file name.
    /// </summary>
    public string NamePdf { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of documents associated with this payment detail.
    /// </summary>
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    /// <summary>
    /// Gets or sets the payment type associated with this payment detail.
    /// </summary>
    public virtual PaymentType IdTipoDePagoNavigation { get; set; } = null!;
}
