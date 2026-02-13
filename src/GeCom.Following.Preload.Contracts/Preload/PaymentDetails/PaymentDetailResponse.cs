namespace GeCom.Following.Preload.Contracts.Preload.PaymentDetails;

/// <summary>
/// Response DTO for PaymentDetail.
/// </summary>
public sealed record PaymentDetailResponse(
    int Id,
    int IdTipoDePago,
    string NroCheque,
    string Banco,
    DateOnly Vencimiento,
    decimal ImporteRecibido,
    DateOnly FechaAlta,
    string NamePdf
);
