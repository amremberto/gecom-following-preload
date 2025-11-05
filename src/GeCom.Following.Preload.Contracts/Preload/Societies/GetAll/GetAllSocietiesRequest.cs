namespace GeCom.Following.Preload.Contracts.Preload.Societies.GetAll;

/// <summary>
/// Request DTO para obtener todas las Societies (preparado para paginación futura).
/// </summary>
public sealed record GetAllSocietiesRequest(int? Page = null, int? PageSize = null);
