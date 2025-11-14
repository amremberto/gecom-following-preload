using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Attachments.DeleteAttachment;

/// <summary>
/// Validator for DeleteAttachmentCommand.
/// </summary>
internal sealed class DeleteAttachmentCommandValidator : AbstractValidator<DeleteAttachmentCommand>
{
    public DeleteAttachmentCommandValidator()
    {
        RuleFor(x => x.AdjuntoId)
            .GreaterThan(0)
            .WithMessage("AdjuntoId must be greater than 0.");
    }
}

