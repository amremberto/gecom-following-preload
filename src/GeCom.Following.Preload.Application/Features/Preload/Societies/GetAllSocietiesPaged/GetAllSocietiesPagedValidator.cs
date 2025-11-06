using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.GetAllSocietiesPaged;

/// <summary>
/// Valida los parámetros de paginación para <see cref="GetAllSocietiesPagedQuery"/>.
/// </summary>
internal sealed class GetAllSocietiesPagedValidator : AbstractValidator<GetAllSocietiesPagedQuery>
{
    public GetAllSocietiesPagedValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(200);
    }
}


