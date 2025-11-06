using GeCom.Following.Preload.Application.Abstractions.Messaging;
using GeCom.Following.Preload.Application.Abstractions.Repositories;
using GeCom.Following.Preload.Application.Mappings;
using GeCom.Following.Preload.Contracts.Preload.Societies;
using GeCom.Following.Preload.Domain.Preloads.Societies;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetSocietyById;

/// <summary>
/// Handler for the GetSocietyByIdQuery.
/// </summary>
internal sealed class GetSocietyByIdQueryHandler : IQueryHandler<GetSocietyByIdQuery, SocietyResponse>
{
    private readonly ISocietyRepository _societyRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSocietyByIdQueryHandler"/> class.
    /// </summary>
    /// <param name="societyRepository">The society repository.</param>
    public GetSocietyByIdQueryHandler(ISocietyRepository societyRepository)
    {
        _societyRepository = societyRepository ?? throw new ArgumentNullException(nameof(societyRepository));
    }

    /// <inheritdoc />
    public async Task<Result<SocietyResponse>> Handle(GetSocietyByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Society? society = await _societyRepository.GetByIdAsync(request.Id, cancellationToken);

        if (society is null)
        {
            return Result.Failure<SocietyResponse>(
                Error.NotFound(
                    "Society.NotFound",
                    $"Society with ID '{request.Id}' was not found."));
        }

        SocietyResponse response = SocietyMappings.ToResponse(society);

        return Result.Success(response);
    }
}

