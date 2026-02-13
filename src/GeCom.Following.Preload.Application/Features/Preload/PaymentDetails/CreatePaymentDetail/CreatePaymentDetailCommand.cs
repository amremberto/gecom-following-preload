using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.PaymentDetails;

namespace GeCom.Following.Preload.Application.Features.Preload.PaymentDetails.CreatePaymentDetail;

/// <summary>
/// Command to create a new payment detail.
/// </summary>
public sealed record CreatePaymentDetailCommand(
    int IdTipoDePago,
    string NroCheque,
    string Banco,
    DateOnly Vencimiento,
    decimal ImporteRecibido,
    string NamePdf,
    DateOnly? FechaAlta = null) : ICommand<PaymentDetailResponse>;
