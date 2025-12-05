namespace GeCom.Following.Preload.WebApp.Middlewares;

/// <summary>
/// Extension methods for registering Identity Server error handling middleware.
/// </summary>
public static class IdentityServerErrorMiddlewareExtensions
{
    /// <summary>
    /// Adds middleware to handle Identity Server connection errors gracefully.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseIdentityServerErrorHandling(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<IdentityServerErrorMiddleware>();
    }
}

