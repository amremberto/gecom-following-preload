using GeCom.Following.Preload.SharedKernel.Results;

namespace GeCom.Following.Preload.WebApi.Extensions.Results;

internal static class ResultExtensions
{
    // Result (sin payload) → IActionResult
    public static IActionResult Match(this Result result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.NoContent();
        }

        return CustomResults.Problem(controller, result);
    }

    // Result<T> (con payload) → IActionResult
    public static IActionResult Match<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return CustomResults.Problem(controller, result);
    }

    // Sobrecargas flexibles si quieres usar lambdas personalizadas:
    public static TOut Match<TOut>(this Result result, Func<TOut> onSuccess, Func<Result, TOut> onFailure)
        => result.IsSuccess ? onSuccess() : onFailure(result);

    public static TOut Match<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> onSuccess, Func<Result<TIn>, TOut> onFailure)
        => result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
}
