namespace GeCom.Following.Preload.Contracts.Preload.Currencies.GetAll;

/// <summary>
/// Request DTO para obtener todas las Currencies (preparado para paginación futura).
/// </summary>
public sealed record GetAllCurrenciesRequest(int? Page = null, int? PageSize = null);

