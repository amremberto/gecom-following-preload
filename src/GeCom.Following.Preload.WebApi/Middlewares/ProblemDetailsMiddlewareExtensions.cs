namespace GeCom.Following.Preload.WebApi.Middlewares;

internal static class ProblemDetailsMiddlewareExtensions
{
    public static IApplicationBuilder UseProblemDetails(this IApplicationBuilder app)
        => app.UseMiddleware<ProblemDetailsMiddleware>();
}
