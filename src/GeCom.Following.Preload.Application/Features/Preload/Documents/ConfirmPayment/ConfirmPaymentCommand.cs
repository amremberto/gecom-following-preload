using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Documents;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.ConfirmPayment;

/// <summary>
/// Command to confirm payment for a document.
/// </summary>
public sealed record ConfirmPaymentCommand(
    int DocId,
    string PaymentMethod,
    string? NumeroCheque,
    string? Banco,
    DateOnly? Vencimiento
) : ICommand<DocumentResponse>;
