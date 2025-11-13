namespace GeCom.Following.Preload.Contracts.Preload.States.Update;

/// <summary>
/// Request DTO for updating an existing state.
/// </summary>
public sealed record UpdateStateRequest(
    string Descripcion,
    string Codigo
);

