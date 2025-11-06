using GeCom.Following.Preload.SharedKernel.Results;
using Microsoft.AspNetCore.Mvc;

namespace GeCom.Following.Preload.WebApi.Extensions.Results;

/// <summary>
/// Extension methods for converting Result types to ActionResult.
/// </summary>
public static class ResultExtensions
{
    #region Result (sin payload) → ActionResult

    /// <summary>
    /// Converts a Result to ActionResult. Returns NoContent on success, Problem on failure.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="controller">The controller instance.</param>
    /// <returns>NoContent (204) on success, Problem on failure.</returns>
    public static ActionResult Match(this Result result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.NoContent();
        }

        return (ActionResult)CustomResults.Problem(controller, result);
    }

    #endregion

    #region Result<T> (con payload) → ActionResult<T>

    /// <summary>
    /// Converts a Result&lt;T&gt; to ActionResult&lt;T&gt;. Returns Ok with value on success, Problem on failure.
    /// Use this for GET requests that return data.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="controller">The controller instance.</param>
    /// <returns>Ok (200) with value on success, Problem on failure.</returns>
    public static ActionResult<T> Match<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return CustomResults.ProblemActionResult(controller, result);
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to ActionResult&lt;T&gt; for POST requests.
    /// Returns CreatedAtAction with the created resource on success, Problem on failure.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="controller">The controller instance.</param>
    /// <param name="actionName">The name of the action to use for generating the URL.</param>
    /// <param name="routeValues">The route values to use for generating the URL.</param>
    /// <returns>CreatedAtAction (201) with value on success, Problem on failure.</returns>
    public static ActionResult<T> MatchCreated<T>(
        this Result<T> result,
        ControllerBase controller,
        string actionName,
        object? routeValues = null)
    {
        if (result.IsSuccess)
        {
            return controller.CreatedAtAction(actionName, routeValues, result.Value);
        }

        return CustomResults.ProblemActionResult(controller, result);
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to ActionResult&lt;T&gt; for POST requests.
    /// Returns CreatedAtRoute with the created resource on success, Problem on failure.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="controller">The controller instance.</param>
    /// <param name="routeName">The name of the route to use for generating the URL.</param>
    /// <param name="routeValues">The route values to use for generating the URL.</param>
    /// <returns>CreatedAtRoute (201) with value on success, Problem on failure.</returns>
    public static ActionResult<T> MatchCreatedAtRoute<T>(
        this Result<T> result,
        ControllerBase controller,
        string routeName,
        object? routeValues = null)
    {
        if (result.IsSuccess)
        {
            return controller.CreatedAtRoute(routeName, routeValues, result.Value);
        }

        return CustomResults.ProblemActionResult(controller, result);
    }

    /// <summary>
    /// Converts a Result&lt;T&gt; to ActionResult&lt;T&gt; for PUT requests.
    /// Returns Ok with value on success, Problem on failure.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="controller">The controller instance.</param>
    /// <returns>Ok (200) with value on success, Problem on failure.</returns>
    public static ActionResult<T> MatchUpdated<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return CustomResults.ProblemActionResult(controller, result);
    }

    /// <summary>
    /// Converts a Result to ActionResult for PUT/PATCH requests that don't return data.
    /// Returns NoContent on success, Problem on failure.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="controller">The controller instance.</param>
    /// <returns>NoContent (204) on success, Problem on failure.</returns>
    public static ActionResult MatchUpdated(this Result result, ControllerBase controller)
        => result.Match(controller);

    /// <summary>
    /// Converts a Result to ActionResult for DELETE requests.
    /// Returns NoContent on success, Problem on failure.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="controller">The controller instance.</param>
    /// <returns>NoContent (204) on success, Problem on failure.</returns>
    public static ActionResult MatchDeleted(this Result result, ControllerBase controller)
        => result.Match(controller);

    #endregion

    #region Sobrecargas flexibles con lambdas personalizadas

    /// <summary>
    /// Matches a Result with custom success and failure handlers.
    /// </summary>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Handler for successful results.</param>
    /// <param name="onFailure">Handler for failed results.</param>
    /// <returns>The result of the appropriate handler.</returns>
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Result, TOut> onFailure)
        => result.IsSuccess ? onSuccess() : onFailure(result);

    /// <summary>
    /// Matches a Result&lt;T&gt; with custom success and failure handlers.
    /// </summary>
    /// <typeparam name="TIn">The input value type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">Handler for successful results.</param>
    /// <param name="onFailure">Handler for failed results.</param>
    /// <returns>The result of the appropriate handler.</returns>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Result<TIn>, TOut> onFailure)
        => result.IsSuccess ? onSuccess(result.Value) : onFailure(result);

    #endregion
}
