namespace GeCom.Following.Preload.SharedKernel.Errors;

/// <summary>
/// Defines the different types of errors that can occur in the application.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Represents a general failure error.
    /// </summary>
    Failure = 0,

    /// <summary>
    /// Represents a validation error that occurs when input data is invalid.
    /// </summary>
    Validation = 1,

    /// <summary>
    /// Represents a problem error that indicates an issue with the operation.
    /// </summary>
    Problem = 2,

    /// <summary>
    /// Represents a not found error when a requested resource does not exist.
    /// </summary>
    NotFound = 3,

    /// <summary>
    /// Represents a conflict error when there's a conflict with the current state.
    /// </summary>
    Conflict = 4
}
