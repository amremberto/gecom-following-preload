using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.UpdateDocumentPdf;

/// <summary>
/// Validator for the UpdateDocumentPdfCommand.
/// </summary>
internal sealed class UpdateDocumentPdfCommandValidator : AbstractValidator<UpdateDocumentPdfCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDocumentPdfCommandValidator"/> class.
    /// </summary>
    public UpdateDocumentPdfCommandValidator()
    {
        RuleFor(x => x.DocId)
            .GreaterThan(0)
            .WithMessage("Document ID must be greater than 0.");

        RuleFor(x => x.FileContent)
            .NotNull()
            .WithMessage("A PDF file is required.");

        RuleFor(x => x.FileContent)
            .Must(content => content is not null && content.Length > 0)
            .When(x => x.FileContent is not null)
            .WithMessage("The file cannot be empty.");

        RuleFor(x => x.FileContent)
            .Must(content => content is not null && content.Length <= 6 * 1024 * 1024) // 6 MB max
            .When(x => x.FileContent is not null)
            .WithMessage("The file size cannot exceed 6 MB.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Content type is required.");

        RuleFor(x => x.UserEmail)
            .NotEmpty()
            .WithMessage("User email is required.");

        RuleFor(x => x)
            .Must(cmd => string.Equals(cmd.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(Path.GetExtension(cmd.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrWhiteSpace(x.ContentType) && !string.IsNullOrWhiteSpace(x.FileName))
            .WithMessage("Only PDF files are allowed.");
    }
}

