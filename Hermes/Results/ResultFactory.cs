namespace Hermes.Results;

/// <summary>
/// Provides factory methods for creating standardized result objects.
/// </summary>
public static class ResultFactory
{
    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value of the successful operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A successful <see cref="Result{T}"/>.</returns>
    public static Result<T> Success<T>(T value, Dictionary<string, string?>? metadata = null)
    {
        return Result<T>.Ok(value, metadata);
    }

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="errorCode">The error code.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Failure<T>(string errorCode, string errorMessage, Dictionary<string, string?>? metadata = null)
    {
        return Result<T>.Failure(errorCode, errorMessage, metadata);
    }

    /// <summary>
    /// Creates a failed result with a single error that includes error-specific metadata.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="error">The error that occurred.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Failure<T>(Error error, Dictionary<string, string?>? metadata = null)
    {
        return Result<T>.Ko(error, metadata);
    }

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="errors">The errors that occurred.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Failure<T>(IEnumerable<Error> errors, Dictionary<string, string?>? metadata = null)
    {
        return Result<T>.Ko(errors, metadata);
    }

    /// <summary>
    /// Creates an error with code, message, and optional error-specific metadata.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="errorMetadata">Optional metadata specific to this error.</param>
    /// <returns>A <see cref="Error"/>.</returns>
    public static Error CreateError(string code, string message, Dictionary<string, string?>? errorMetadata = null)
    {
        return new Error(code, message, errorMetadata);
    }
}
