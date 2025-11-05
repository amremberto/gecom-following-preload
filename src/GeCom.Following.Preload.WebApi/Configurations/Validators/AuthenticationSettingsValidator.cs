using GeCom.Following.Preload.WebAPI.Configurations.Settings;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebAPI.Configurations.Validators;

/// <summary>
/// Validates AuthenticationSettings values.
/// </summary>
internal sealed class AuthenticationSettingsValidator : IValidateOptions<AuthenticationSettings>
{
    public ValidateOptionsResult Validate(string? name, AuthenticationSettings options)
    {
        if (options is null)
        {
            return ValidateOptionsResult.Fail("Authentication settings are missing.");
        }

        List<string> failures = new();

        if (string.IsNullOrWhiteSpace(options.Authority))
        {
            failures.Add("Authority must be provided.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add("Audience must be provided.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}


