namespace GeCom.Following.Preload.Contracts.Preload.PaymentTypes.Create;

/// <summary>
/// Request DTO for creating a new payment type.
/// </summary>
public sealed record CreatePaymentTypeRequest(
    string Descripcion
);

