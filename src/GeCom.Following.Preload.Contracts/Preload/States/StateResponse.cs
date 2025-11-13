namespace GeCom.Following.Preload.Contracts.Preload.States;

/// <summary>
/// Response DTO for State.
/// </summary>
public sealed record StateResponse(
    int EstadoId,
    string Descripcion,
    string Codigo,
    DateTime FechaCreacion,
    DateTime? FechaBaja
);

