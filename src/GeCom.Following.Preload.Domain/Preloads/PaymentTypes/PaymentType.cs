using GeCom.Following.Preload.Domain.Preloads.PaymentDetails;
using GeCom.Following.Preload.SharedKernel.Entities;

namespace GeCom.Following.Preload.Domain.Preloads.PaymentTypes;

/// <summary>
/// Represents a payment type entity in the preload system.
/// </summary>
/// <remarks>
/// This entity defines the different types of payments that can be processed,
/// such as cash, check, transfer, etc.
/// </remarks>
public partial class PaymentType : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique payment type identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the description of this payment type.
    /// </summary>
    public string Descripcion { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of payment details that use this payment type.
    /// </summary>
    public virtual ICollection<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();
}
