using FluentValidation;

namespace GeCom.Following.Preload.Application.Features.Preload.Societies.DeleteSociety;

/// <summary>
/// Validator for DeleteSocietyCommand.
/// </summary>
internal sealed class DeleteSocietyCommandValidator : AbstractValidator<DeleteSocietyCommand>
{
    public DeleteSocietyCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than zero.");
    }
}

