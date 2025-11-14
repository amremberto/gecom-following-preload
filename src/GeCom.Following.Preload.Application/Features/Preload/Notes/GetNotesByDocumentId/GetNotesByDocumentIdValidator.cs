using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Notes.GetNotesByDocumentId;

/// <summary>
/// Validator for GetNotesByDocumentIdQuery.
/// </summary>
internal sealed class GetNotesByDocumentIdValidator : AbstractValidator<GetNotesByDocumentIdQuery>
{
    public GetNotesByDocumentIdValidator()
    {
        RuleFor(x => x.DocId)
            .GreaterThan(0)
            .WithMessage("DocId must be greater than 0.");
    }
}

