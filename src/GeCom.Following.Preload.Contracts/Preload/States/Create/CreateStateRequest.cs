namespace GeCom.Following.Preload.Contracts.Preload.States.Create;

/// <summary>
/// Request DTO for creating a new state.
/// </summary>
public sealed record CreateStateRequest(
    string Descripcion,
    string Codigo
);

