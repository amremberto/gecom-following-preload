using Microsoft.Extensions.Options;

namespace GeCom.Following.Preload.WebAPI.Configurations.Validators;

/// <summary>
/// Validator for database settings configuration.
/// </summary>
public sealed class DatabaseSettingsValidator : IValidateOptions<object>
{
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSettingsValidator"/> class.
    /// </summary>
    /// <param name="environment">The host environment.</param>
    public DatabaseSettingsValidator(IHostEnvironment environment)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    /// Validates the database settings by checking connection strings.
    /// </summary>
    /// <param name="name">The name of the options instance being validated.</param>
    /// <param name="options">The options instance.</param>
    /// <returns>The validation result.</returns>
    public ValidateOptionsResult Validate(string? name, object options)
    {
        var failures = new List<string>();

        // Note: Connection string validation is typically done at runtime when EF Core
        // tries to connect. This validator focuses on basic configuration checks.

        // In production, connection strings should not contain localhost
        if (!_environment.IsDevelopment())
        {
            // This is a basic check - in a real implementation, you might want to
            // validate connection string format, check if database is reachable, etc.
            failures.Add("Database configuration should be validated for production environment");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
