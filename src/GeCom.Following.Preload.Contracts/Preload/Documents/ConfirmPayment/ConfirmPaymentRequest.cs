namespace GeCom.Following.Preload.Contracts.Preload.Documents.ConfirmPayment;

/// <summary>
/// Request DTO for confirming payment on a document.
/// </summary>
public sealed record ConfirmPaymentRequest(
    string PaymentMethod,
    string? NumeroCheque,
    string? Banco,
    DateOnly? Vencimiento
);
