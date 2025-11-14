using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Attachments.UpdateAttachment;

/// <summary>
/// Validator for UpdateAttachmentCommand.
/// </summary>
internal sealed class UpdateAttachmentCommandValidator : AbstractValidator<UpdateAttachmentCommand>
{
    public UpdateAttachmentCommandValidator()
    {
        RuleFor(x => x.AdjuntoId)
            .GreaterThan(0)
            .WithMessage("AdjuntoId must be greater than 0.");

        RuleFor(x => x.Path)
            .NotEmpty()
            .WithMessage("Path is required.")
            .MaximumLength(500)
            .WithMessage("Path must not exceed 500 characters.");
    }
}

