namespace GeCom.Following.Preload.Contracts.Preload.PaymentTypes;

/// <summary>
/// Response DTO for PaymentType.
/// </summary>
public sealed record PaymentTypeResponse(
    int Id,
    string Descripcion
);

