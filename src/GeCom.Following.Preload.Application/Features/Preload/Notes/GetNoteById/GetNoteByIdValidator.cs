using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.GetNoteById;

/// <summary>
/// Validator for GetNoteByIdQuery.
/// </summary>
internal sealed class GetNoteByIdValidator : AbstractValidator<GetNoteByIdQuery>
{
    public GetNoteByIdValidator()
    {
        RuleFor(x => x.NotaId)
            .GreaterThan(0)
            .WithMessage("NotaId must be greater than 0.");
    }
}

