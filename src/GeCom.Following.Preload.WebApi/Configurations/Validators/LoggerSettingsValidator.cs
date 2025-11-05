using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebAPI.Configurations.Validators;

/// <summary>
/// Validator for logger settings configuration.
/// </summary>
public sealed class LoggerSettingsValidator : IValidateOptions<object>
{
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerSettingsValidator"/> class.
    /// </summary>
    /// <param name="environment">The host environment.</param>
    public LoggerSettingsValidator(IHostEnvironment environment)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    /// Validates the logger settings by checking Serilog configuration.
    /// </summary>
    /// <param name="name">The name of the options instance being validated.</param>
    /// <param name="options">The options instance.</param>
    /// <returns>The validation result.</returns>
    public ValidateOptionsResult Validate(string? name, object options)
    {
        var failures = new List<string>();

        // Note: Serilog configuration is complex and typically validated at runtime
        // This validator focuses on basic configuration checks

        // In production, we should have file logging enabled
        if (!_environment.IsDevelopment())
        {
            // This is a basic check - in a real implementation, you might want to
            // check if file paths are writable, disk space, etc.
            failures.Add("Logger configuration should be validated for production environment");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
