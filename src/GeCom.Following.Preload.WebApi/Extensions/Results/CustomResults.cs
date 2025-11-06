using System.Diagnostics;
using GeCom.Following.Preload.SharedKernel.Errors;
using GeCom.Following.Preload.SharedKernel.Results;


//using Microsoft.AspNetCore.Mvc;          // IActionResult, ControllerBase, ProblemDetails
using Microsoft.Extensions.Primitives;

namespace GeCom.Following.Preload.WebApi.Extensions.Results;

internal static class CustomResults
{
    public static IActionResult Problem(ControllerBase controller, Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Problem called with a successful result.");
        }

        Error error = result.Error;

        int status = GetStatusCode(error.Type);

        ProblemDetails pd = new()
        {
            Title = GetTitle(error),
            Detail = GetDetail(error),
            Type = GetTypeLink(error.Type),
            Status = status,
            Instance = controller.HttpContext?.Request?.Path.Value
        };

        string? traceId = Activity.Current?.Id ?? controller.HttpContext?.TraceIdentifier;
        if (!string.IsNullOrWhiteSpace(traceId))
        {
            pd.Extensions["traceId"] = traceId;
        }

        if (controller.HttpContext?.Request?.Headers.TryGetValue("X-Correlation-ID", out StringValues corr) is true)
        {
            pd.Extensions["correlationId"] = corr.ToString();
        }

        // Si el Error concreto trae errores de validación, añádelos como extensión estándar.
        if (error is ValidationError validation)
        {
            pd.Extensions["errors"] = validation.Errors;
        }

        return new ObjectResult(pd)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" }
        };
    }

    public static IActionResult Problem<T>(ControllerBase controller, Result<T> result)
        => Problem(controller, (Result)result);

    // --- helpers privados ---

    private static string GetTitle(Error error) =>
        error.Code ?? error.Type.ToString();

    private static string GetDetail(Error error) =>
        string.IsNullOrWhiteSpace(error.Description)
            ? "An unexpected error occurred."
            : error.Description;

    private static string GetTypeLink(ErrorType type) => type switch
    {
        ErrorType.Validation => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
        ErrorType.NotFound => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
        ErrorType.Conflict => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
        ErrorType.Unauthorized => "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
        ErrorType.Forbidden => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
        _ => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"
    };

    private static int GetStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status500InternalServerError
    };
}
