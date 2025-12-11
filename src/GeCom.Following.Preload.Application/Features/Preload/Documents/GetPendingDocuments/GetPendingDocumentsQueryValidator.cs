using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Documents.GetPendingDocuments;

/// <summary>
/// Validator for GetPendingDocumentsQuery.
/// </summary>
internal sealed class GetPendingDocumentsQueryValidator
    : AbstractValidator<GetPendingDocumentsQuery>
{
    public GetPendingDocumentsQueryValidator()
    {
        RuleFor(x => x.UserRoles)
            .NotNull()
            .WithMessage("User roles are required.")
            .NotEmpty()
            .WithMessage("At least one user role is required.");

        RuleFor(x => x.UserEmail)
            .NotEmpty()
            .WithMessage("User email is required.")
            .NotNull()
            .WithMessage("User email cannot be null.")
            .EmailAddress()
            .WithMessage("User email must be a valid email address.");
    }
}

