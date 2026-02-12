using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.DeleteDocument;

/// <summary>
/// Validator for DeleteDocumentCommand.
/// </summary>
internal sealed class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentCommandValidator()
    {
        RuleFor(x => x.DocId)
            .GreaterThan(0)
            .WithMessage("DocId must be greater than 0.");
    }
}
