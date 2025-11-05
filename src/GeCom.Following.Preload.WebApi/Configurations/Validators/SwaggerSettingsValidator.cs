using GeCom.Following.Preload.WebAPI.Configurations.Settings;
using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebAPI.Configurations.Validators;

/// <summary>
/// Validator for Swagger settings configuration.
/// </summary>
public sealed class SwaggerSettingsValidator : IValidateOptions<ApiSwaggerSettings>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, ApiSwaggerSettings options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Title))
        {
            failures.Add("Swagger Title is required");
        }

        if (string.IsNullOrWhiteSpace(options.Version))
        {
            failures.Add("Swagger Version is required");
        }

        if (string.IsNullOrWhiteSpace(options.DocumentName))
        {
            failures.Add("Swagger DocumentName is required");
        }

        if (string.IsNullOrWhiteSpace(options.Route) || !options.Route.StartsWith('/'))
        {
            failures.Add("Swagger Route must start with '/'");
        }

        if (string.IsNullOrWhiteSpace(options.JsonRoute) || !options.JsonRoute.StartsWith('/'))
        {
            failures.Add("Swagger JsonRoute must start with '/'");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}


