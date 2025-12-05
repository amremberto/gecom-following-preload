using System.Net.Sockets;

namespace GeCom.Following.Preload.WebApp.Middlewares;

/// <summary>
/// Middleware to catch and handle Identity Server connection errors gracefully.
/// </summary>
internal sealed class IdentityServerErrorMiddleware(RequestDelegate next, ILogger<IdentityServerErrorMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<IdentityServerErrorMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (IsIdentityServerError(ex))
        {
            _logger.LogError(
                ex,
                "Error de conexión con el servidor de identidad al procesar {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            // Evitar redirección infinita
            if (context.Request.Path.StartsWithSegments("/identity-server-error", StringComparison.OrdinalIgnoreCase))
            {
                // Si ya estamos en la página de error, dejar que el error se propague
                throw;
            }

            // Redirigir a la página de error amigable
            context.Response.Redirect("/identity-server-error");
        }
    }

    /// <summary>
    /// Determina si la excepción está relacionada con problemas de conexión al servidor de identidad.
    /// </summary>
    private static bool IsIdentityServerError(Exception ex)
    {
        // Verificar InvalidOperationException con IDX20803 (Unable to obtain configuration)
        if (ex is InvalidOperationException invalidOpEx && (invalidOpEx.Message.Contains("IDX20803", StringComparison.OrdinalIgnoreCase) ||
                invalidOpEx.Message.Contains("Unable to obtain configuration", StringComparison.OrdinalIgnoreCase) ||
                invalidOpEx.Message.Contains("openid-configuration", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Verificar IOException con IDX20804 (Unable to retrieve document)
        if (ex is IOException ioEx && (ioEx.Message.Contains("IDX20804", StringComparison.OrdinalIgnoreCase) ||
                ioEx.Message.Contains("Unable to retrieve document", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Verificar HttpRequestException (problemas de conexión HTTP)
        if (ex is HttpRequestException httpEx && (httpEx.Message.Contains("No se puede establecer una conexión", StringComparison.OrdinalIgnoreCase) ||
                httpEx.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
                httpEx.Message.Contains("denegó", StringComparison.OrdinalIgnoreCase) ||
                httpEx.Message.Contains("refused", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Verificar SocketException (error 10061 = WSAECONNREFUSED)
        if (ex is SocketException socketEx && (socketEx.ErrorCode == 10061 ||
                socketEx.Message.Contains("No se puede establecer una conexión", StringComparison.OrdinalIgnoreCase) ||
                socketEx.Message.Contains("denegó", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Verificar excepciones internas (wrapped exceptions)
        Exception? innerEx = ex.InnerException;
        while (innerEx is not null)
        {
            if (IsIdentityServerError(innerEx))
            {
                return true;
            }

            innerEx = innerEx.InnerException;
        }

        return false;
    }
}

