using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietyByCodigo;

/// <summary>
/// Handler for the GetSocietyByCodigoQuery.
/// </summary>
internal sealed class GetSocietyByCodigoQueryHandler : IQueryHandler<GetSocietyByCodigoQuery, SocietyResponse>
{
    private readonly ISocietyRepository _societyRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSocietyByCodigoQueryHandler"/> class.
    /// </summary>
    /// <param name="societyRepository">The society repository.</param>
    public GetSocietyByCodigoQueryHandler(ISocietyRepository societyRepository)
    {
        _societyRepository = societyRepository ?? throw new ArgumentNullException(nameof(societyRepository));
    }

    /// <inheritdoc />
    public async Task<Result<SocietyResponse>> Handle(GetSocietyByCodigoQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Domain.Preloads.Societies.Society? society = await _societyRepository.GetByCodigoAsync(request.Codigo, cancellationToken);

        if (society is null)
        {
            return Result.Failure<SocietyResponse>(
                Error.NotFound(
                    "Society.NotFound",
                    $"Society with code '{request.Codigo}' was not found."));
        }

        SocietyResponse response = SocietyMappings.ToResponse(society);

        return Result.Success(response);
    }
}

