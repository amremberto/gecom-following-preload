namespace GeCom.Following.Preload.Contracts.Preload.Societies;

/// <summary>
/// DTO for Society select dropdown items.
/// </summary>
public sealed record SocietySelectItem(
    string Cuit,
    string Descripcion
);

