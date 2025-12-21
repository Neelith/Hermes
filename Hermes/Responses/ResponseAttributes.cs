namespace Hermes.Responses;

/// <summary>
/// Contains constant string values for common response attributes used in API responses.
/// </summary>
public static class ResponseAttributes
{
    /// <summary>
    /// The attribute name for the total count of items in a collection response.
    /// </summary>
    public const string TotalCount = "totalCount";

    /// <summary>
    /// The attribute name for the type of the response or resource.
    /// </summary>
    public const string Type = "type";
}

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