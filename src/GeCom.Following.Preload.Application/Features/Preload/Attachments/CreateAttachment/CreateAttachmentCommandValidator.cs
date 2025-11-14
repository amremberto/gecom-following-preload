using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Attachments.CreateAttachment;

/// <summary>
/// Validator for CreateAttachmentCommand.
/// </summary>
internal sealed class CreateAttachmentCommandValidator : AbstractValidator<CreateAttachmentCommand>
{
    public CreateAttachmentCommandValidator()
    {
        RuleFor(x => x.Path)
            .NotEmpty()
            .WithMessage("Path is required.")
            .MaximumLength(500)
            .WithMessage("Path must not exceed 500 characters.");

        RuleFor(x => x.DocId)
            .GreaterThan(0)
            .WithMessage("DocId must be greater than 0.");
    }
}

