namespace GeCom.Following.Preload.SharedKernel.Errors;

/// <summary>
/// Represents an error that occurred during operation execution.
/// </summary>
/// <remarks>This record encapsulates error information including a code, description, and type.
/// It provides static factory methods for creating common error types and predefined error instances.</remarks>
public record Error
{
    /// <summary>
    /// Represents a successful operation with no error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    /// <summary>
    /// Represents an error when a null value was provided where it was not expected.
    /// </summary>
    public static readonly Error NullValue = new(
        "General.Null",
        "Null value was provided",
        ErrorType.Failure);

    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="code">The error code that uniquely identifies this error.</param>
    /// <param name="description">A human-readable description of the error.</param>
    /// <param name="type">The type of error that occurred.</param>
    public Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    /// <summary>
    /// Gets the error code that uniquely identifies this error.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets a human-readable description of the error.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the type of error that occurred.
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// Creates a new failure error with the specified code and description.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A new <see cref="Error"/> instance with type <see cref="ErrorType.Failure"/>.</returns>
    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    /// <summary>
    /// Creates a new not found error with the specified code and description.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A new <see cref="Error"/> instance with type <see cref="ErrorType.NotFound"/>.</returns>
    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    /// <summary>
    /// Creates a new problem error with the specified code and description.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A new <see cref="Error"/> instance with type <see cref="ErrorType.Problem"/>.</returns>
    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    /// <summary>
    /// Creates a new conflict error with the specified code and description.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A new <see cref="Error"/> instance with type <see cref="ErrorType.Conflict"/>.</returns>
    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);
}
