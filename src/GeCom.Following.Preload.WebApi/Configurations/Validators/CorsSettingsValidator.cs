using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebAPI.Configurations.Validators;

/// <summary>
/// Validator for CORS settings configuration.
/// </summary>
public sealed class CorsSettingsValidator : IValidateOptions<Settings.CorsSettings>
{
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorsSettingsValidator"/> class.
    /// </summary>
    /// <param name="environment">The host environment.</param>
    public CorsSettingsValidator(IHostEnvironment environment)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    /// Validates the CORS settings.
    /// </summary>
    /// <param name="name">The name of the options instance being validated.</param>
    /// <param name="options">The options instance.</param>
    /// <returns>The validation result.</returns>
    public ValidateOptionsResult Validate(string? name, Settings.CorsSettings options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        // Validate PolicyName
        if (string.IsNullOrWhiteSpace(options.PolicyName))
        {
            failures.Add("CORS PolicyName is required");
        }

        // Validate AllowedOrigins in non-development environments
        if (!_environment.IsDevelopment())
        {
            if (options.AllowedOrigins.Length == 0)
            {
                failures.Add("CORS AllowedOrigins must be specified in non-development environments");
            }
            else
            {
                // Validate origin URLs
                foreach (string origin in options.AllowedOrigins)
                {
                    if (string.IsNullOrWhiteSpace(origin))
                    {
                        failures.Add("CORS AllowedOrigins cannot contain empty or null values");
                        break;
                    }

                    if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri))
                    {
                        failures.Add($"CORS AllowedOrigins contains invalid URL: {origin}");
                    }
                    else if (uri.Scheme != "http" && uri.Scheme != "https")
                    {
                        failures.Add($"CORS AllowedOrigins must use http or https scheme: {origin}");
                    }
                }
            }
        }

        // Validate AllowedMethods
        if (options.AllowedMethods.Length > 0)
        {
            string[]? validMethods = ["GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS"];
            foreach (string method in options.AllowedMethods)
            {
                if (string.IsNullOrWhiteSpace(method))
                {
                    failures.Add("CORS AllowedMethods cannot contain empty or null values");
                    break;
                }

                if (!validMethods.Contains(method.ToUpperInvariant()))
                {
                    failures.Add($"CORS AllowedMethods contains invalid HTTP method: {method}");
                }
            }
        }

        // Validate AllowCredentials with AnyOrigin conflict
        if (options.AllowCredentials && _environment.IsDevelopment())
        {
            // In development, we use AllowAnyOrigin() which conflicts with AllowCredentials
            // This is a warning, not an error, but we should log it
            // Note: This is handled in the CORS configuration by not setting AllowCredentials in dev
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
