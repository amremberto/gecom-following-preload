using GeCom.Following.Preload.SharedKernel.Errors;

namespace GeCom.Following.Preload.SharedKernel.Results;

/// <summary>
/// Represents a validation error that contains multiple validation errors.
/// </summary>
/// <remarks>This sealed record extends <see cref="Error"/> to represent validation failures
/// that contain multiple individual validation errors. It provides a convenient way to aggregate
/// multiple validation failures into a single error instance.</remarks>
public sealed record ValidationError : Error
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationError"/> class.
    /// </summary>
    /// <param name="errors">An array of individual validation errors.</param>
    public ValidationError(Error[] errors)
        : base(
            "Validation.General",
            "One or more validation errors occurred",
            ErrorType.Validation)
    {
        Errors = errors;
    }

    /// <summary>
    /// Gets the array of individual validation errors.
    /// </summary>
    public Error[] Errors { get; }

    /// <summary>
    /// Creates a validation error from a collection of failed results.
    /// </summary>
    /// <param name="results">The collection of results to extract errors from.</param>
    /// <returns>A new <see cref="ValidationError"/> containing all the errors from failed results.</returns>
    public static ValidationError FromResults(IEnumerable<Result> results) =>
        new(results.Where(r => r.IsFailure).Select(r => r.Error).ToArray());
}
