using System.Diagnostics.CodeAnalysis;
using GeCom.Following.Preload.SharedKernel.Errors;

namespace GeCom.Following.Preload.SharedKernel.Results;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// </summary>
/// <remarks>This class provides a functional approach to error handling by encapsulating
/// both success and failure states along with error information when applicable.</remarks>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error information if the operation failed, or <see cref="Error.None"/> if successful.</param>
    /// <exception cref="ArgumentException">Thrown when the success state and error state are inconsistent.</exception>
    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error information associated with this result.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Creates a successful result with no value.
    /// </summary>
    /// <returns>A new successful <see cref="Result"/> instance.</returns>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value to include in the result.</param>
    /// <returns>A new successful <see cref="Result{TValue}"/> instance containing the specified value.</returns>
    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error information.</param>
    /// <returns>A new failed <see cref="Result"/> instance.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result with the specified error and no value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="error">The error information.</param>
    /// <returns>A new failed <see cref="Result{TValue}"/> instance.</returns>
    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);
}

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail with an error.
/// </summary>
/// <typeparam name="TValue">The type of the value returned on success.</typeparam>
/// <remarks>This generic class extends <see cref="Result"/> to include a value when the operation succeeds.
/// It provides type-safe access to the result value and includes implicit conversion operators for convenience.</remarks>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value to include in the result.</param>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error information if the operation failed, or <see cref="Error.None"/> if successful.</param>
    public Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the value of the result.
    /// </summary>
    /// <value>The value if the operation was successful.</value>
    /// <exception cref="InvalidOperationException">Thrown when attempting to access the value of a failed result.</exception>
    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A successful result containing the value, or a failed result if the value is null.</returns>
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    /// <summary>
    /// Creates a validation failure result with the specified error.
    /// </summary>
    /// <param name="error">The validation error information.</param>
    /// <returns>A new failed <see cref="Result{TValue}"/> instance with validation error type.</returns>
    public static Result<TValue> ValidationFailure(Error error) =>
        new(default, false, error);
}
