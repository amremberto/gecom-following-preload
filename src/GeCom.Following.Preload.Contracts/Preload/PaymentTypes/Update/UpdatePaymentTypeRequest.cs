namespace GeCom.Following.Preload.Contracts.Preload.PaymentTypes.Update;

/// <summary>
/// Request DTO for updating an existing payment type.
/// </summary>
public sealed record UpdatePaymentTypeRequest(
    string Descripcion
);

