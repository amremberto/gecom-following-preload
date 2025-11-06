using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietyByCuit;

/// <summary>
/// Handler for the GetSocietyByCuitQuery.
/// </summary>
internal sealed class GetSocietyByCuitQueryHandler : IQueryHandler<GetSocietyByCuitQuery, SocietyResponse>
{
    private readonly ISocietyRepository _societyRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSocietyByCuitQueryHandler"/> class.
    /// </summary>
    /// <param name="societyRepository">The society repository.</param>
    public GetSocietyByCuitQueryHandler(ISocietyRepository societyRepository)
    {
        _societyRepository = societyRepository ?? throw new ArgumentNullException(nameof(societyRepository));
    }

    /// <inheritdoc />
    public async Task<Result<SocietyResponse>> Handle(GetSocietyByCuitQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Domain.Preloads.Societies.Society? society = await _societyRepository.GetByCuitAsync(request.Cuit, cancellationToken);

        if (society is null)
        {
            return Result.Failure<SocietyResponse>(
                Error.NotFound(
                    "Society.NotFound",
                    $"Society with CUIT '{request.Cuit}' was not found."));
        }

        SocietyResponse response = SocietyMappings.ToResponse(society);

        return Result.Success(response);
    }
}

