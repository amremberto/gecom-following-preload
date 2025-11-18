namespace GeCom.Following.Preload.WebApp.Services;

/// <summary>
/// Exception thrown when an HTTP request fails with a specific error message from the API.
/// </summary>
public sealed class ApiRequestException : Exception
{
    /// <summary>
    /// Gets the HTTP status code of the response.
    /// </summary>
    public System.Net.HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiRequestException"/> class.
    /// </summary>
    public ApiRequestException()
        : base()
    {
        StatusCode = System.Net.HttpStatusCode.InternalServerError;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiRequestException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ApiRequestException(string message)
        : base(message)
    {
        StatusCode = System.Net.HttpStatusCode.InternalServerError;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiRequestException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ApiRequestException(string message, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = System.Net.HttpStatusCode.InternalServerError;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiRequestException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="message">The error message.</param>
    public ApiRequestException(System.Net.HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiRequestException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ApiRequestException(System.Net.HttpStatusCode statusCode, string message, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}

