using System.Diagnostics;
using GeCom.Following.Preload.SharedKernel.Errors;
using Microsoft.Extensions.Primitives;

namespace GeCom.Following.Preload.WebApi.Extensions.ApiProblems;

public static class ProblemDetailsExtensions
{
    public static IActionResult Problem(this ControllerBase controller, Error error)
    {
        int status = ErrorHttpStatusCodeMapper.ToStatusCode(error.Type);

        ProblemDetails pd = new()
        {
            Type = $"https://httpstatuses.com/{status}",
            Status = status,
            Title = error.Code,
            Detail = error.Description,
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

        return new ObjectResult(pd) { StatusCode = status, ContentTypes = { "application/problem+json" } };
    }
}
