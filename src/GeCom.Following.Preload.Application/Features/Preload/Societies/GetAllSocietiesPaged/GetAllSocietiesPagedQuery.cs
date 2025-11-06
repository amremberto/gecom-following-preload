using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetAllSocietiesPaged;

/// <summary>
/// Query para obtener Societies paginadas.
/// </summary>
public sealed record GetAllSocietiesPagedQuery(int Page, int PageSize) : IQuery<PagedResponse<SocietyResponse>>;


