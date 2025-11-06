using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Primitives;

namespace GeCom.Following.Preload.WebApi.Middlewares;

internal sealed class ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly RequestDelegate _next = next;
    private readonly ILogger<ProblemDetailsMiddleware> _logger = logger;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Petición cancelada por el cliente: no spamear logs de error
            context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest; // no estándar, a veces útil
        }
        catch (Exception ex)
        {
            // Log estructurado UNA sola vez aquí (evitamos duplicados en behaviors)
            _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);

            await WriteProblemDetailsAsync(context, ex);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext ctx, Exception ex)
    {
        int status = (int)HttpStatusCode.InternalServerError;

        var pd = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{status}",
            Title = "An unexpected error occurred.",
            Detail = ex.Message,
            Status = status,
            Instance = ctx.Request.Path
        };

        string? traceId = Activity.Current?.Id ?? ctx.TraceIdentifier;
        if (!string.IsNullOrWhiteSpace(traceId))
        {
            pd.Extensions["traceId"] = traceId;
        }

        if (ctx.Request.Headers.TryGetValue("X-Correlation-ID", out StringValues corr))
        {
            pd.Extensions["correlationId"] = corr.ToString();
        }

        ctx.Response.Clear();
        ctx.Response.ContentType = "application/problem+json";
        ctx.Response.StatusCode = status;
        await JsonSerializer.SerializeAsync(ctx.Response.Body, pd, JsonOptions, ctx.RequestAborted);
    }
}
