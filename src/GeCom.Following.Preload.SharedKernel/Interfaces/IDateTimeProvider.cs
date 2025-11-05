namespace GeCom.Following.Preload.SharedKernel.Interfaces;

/// <summary>
/// Provides access to the current date and time in a testable manner.
/// </summary>
/// <remarks>This interface abstracts the system clock to enable unit testing and time-based operations
/// that can be controlled in test scenarios.</remarks>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current date and time in UTC.
    /// </summary>
    /// <value>The current date and time in UTC.</value>
    DateTime UtcNow { get; }
}
