namespace Hermes.Results;

/// <summary>
/// Provides extension methods for Result types to enable functional composition and chaining.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a successful result to a different type by transforming its value.
    /// </summary>
    /// <typeparam name="TSource">The source value type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="selector">The transformation function.</param>
    /// <returns>A new result with the transformed value or the original errors.</returns>
    public static Result<TResult> Map<TSource, TResult>(this Result<TSource> result, Func<TSource?, TResult> selector)
    {
        return result.IsSuccess
            ? Result<TResult>.Ok(selector(result.Value), result.Metadata)
            : Result<TResult>.Ko(result.Errors, result.Metadata);
    }

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="action">The action to execute.</param>
    /// <returns>The original result for chaining.</returns>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T?> action)
    {
        if (result.IsSuccess)
        {
            action(result.Value);
        }
        return result;
    }

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="action">The action to execute.</param>
    /// <returns>The original result for chaining.</returns>
    public static Result OnSuccess(this Result result, Action action)
    {
        if (result.IsSuccess)
        {
            action();
        }
        return result;
    }

    /// <summary>
    /// Executes an action if the result is failed.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="action">The action to execute with the errors.</param>
    /// <returns>The original result for chaining.</returns>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<IReadOnlyList<IError>> action)
    {
        if (result.IsFailure)
        {
            action(result.Errors);
        }
        return result;
    }

    /// <summary>
    /// Executes an action if the result is failed.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="action">The action to execute with the errors.</param>
    /// <returns>The original result for chaining.</returns>
    public static Result OnFailure(this Result result, Action<IReadOnlyList<IError>> action)
    {
        if (result.IsFailure)
        {
            action(result.Errors);
        }
        return result;
    }
}
