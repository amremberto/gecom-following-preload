using GeCom.Following.Preload.WebApp.Configurations.Settings;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebApp.Configurations.Validators;

/// <summary>
/// Validates IdentityServerSettings values.
/// </summary>
internal sealed class IdentityServerSettingsValidator : IValidateOptions<IdentityServerSettings>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, IdentityServerSettings options)
    {
        if (options is null)
        {
            return ValidateOptionsResult.Fail("IdentityServer settings are missing.");
        }

        List<string> failures = new();

        if (string.IsNullOrWhiteSpace(options.Authority))
        {
            failures.Add("Authority must be provided.");
        }

        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            failures.Add("ClientId must be provided.");
        }

        if (options.RequiredScopes is null || options.RequiredScopes.Length == 0)
        {
            failures.Add("At least one RequiredScopes value must be provided.");
        }

        if (string.IsNullOrWhiteSpace(options.RedirectUri))
        {
            failures.Add("RedirectUri must be provided.");
        }

        if (string.IsNullOrWhiteSpace(options.PostLogoutRedirectUri))
        {
            failures.Add("PostLogoutRedirectUri must be provided.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}

