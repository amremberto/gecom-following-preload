namespace GeCom.Following.Preload.SharedKernel.Errors;

/// <summary>
/// Defines the different types of errors that can occur in the application.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Represents the absence of a specific value or option.
    /// </summary>
    /// <remarks>This status code indicates that no error has occurred or that no specific error type
    /// is applicable.</remarks>
    None = 0,

    /// <summary>
    /// Represents a validation error that occurs when input data is invalid.
    /// </summary>
    /// <remarks>This status code indicates that the server cannot process the request due to
    /// a client error (e.g., malformed request syntax, invalid request message framing,
    /// or deceptive request routing).</remarks>
    Validation = 1,

    /// <summary>
    /// Represents a not found error when a requested resource does not exist.
    /// </summary>
    /// <remarks>This status code indicates that the server cannot find the requested resource.
    /// Typically used when the resource has been deleted or does not exist.</remarks>
    NotFound = 2,

    /// <summary>
    /// Represents a conflict error when there's a conflict with the current state.
    /// </summary>
    /// <remarks>This status code indicates that the request could not be completed due to a conflict
    /// with the current state of the target resource. Typically used in situations where the user
    /// might be able to resolve the conflict and resubmit the request.</remarks>
    Conflict = 3,

    /// <summary>
    /// Represents the status of an operation that was not authorized.
    /// </summary>
    /// <remarks> This status indicates that the client must authenticate itself to get the requested response.
    /// Typically used when authentication is required and has failed or has not yet been provided.</remarks>
    Unauthorized = 4,

    /// <summary>
    /// Represents the HTTP status code for a forbidden request.
    /// </summary>
    /// <remarks>This status code indicates that the server understood the request but refuses to authorize
    /// it. Typically used when the client does not have permission to access the requested resource.</remarks>
    Forbidden = 5,

    /// <summary>
    /// Represents a problem error that indicates an issue with the operation.
    /// </summary>
    Problem = 6,

    /// <summary>
    /// Represents a general failure error.
    /// </summary>
    /// <remarks>This status code indicates that the server encountered an unexpected condition
    /// that prevented it from fulfilling the request. Typically used for server errors.</remarks>
    Failure = 7,
}
