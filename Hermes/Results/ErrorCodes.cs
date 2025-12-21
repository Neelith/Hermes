namespace Hermes.Results;

/// <summary>
/// Contains constant string values for common error codes.
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    /// General validation error.
    /// </summary>
    public const string ValidationError = "VALIDATION_ERROR";

    /// <summary>
    /// Resource not found error.
    /// </summary>
    public const string NotFound = "NOT_FOUND";

    /// <summary>
    /// Unauthorized access error.
    /// </summary>
    public const string Unauthorized = "UNAUTHORIZED";

    /// <summary>
    /// Internal server error.
    /// </summary>
    public const string InternalError = "INTERNAL_ERROR";

    /// <summary>
    /// Invalid operation error.
    /// </summary>
    public const string InvalidOperation = "INVALID_OPERATION";

    /// <summary>
    /// Conflict error.
    /// </summary>
    public const string Conflict = "CONFLICT";
}