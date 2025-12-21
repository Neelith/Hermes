namespace Hermes.Results;

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail with errors.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public class Result<T>
{
    private readonly T? _value;
    private readonly List<IError> _errors;

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the value if the operation was successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when attempting to access the value of a failed result.</exception>
    public T Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException("Cannot access Value of a failed result. Check IsSuccess before accessing Value.");
            }
            return _value!;
        }
    }

    /// <summary>
    /// Gets the collection of errors if the operation failed.
    /// </summary>
    public IReadOnlyList<IError> Errors => _errors.AsReadOnly();

    /// <summary>
    /// Gets or sets optional metadata associated with this result.
    /// </summary>
    public Dictionary<string, string?>? Metadata { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class for a successful operation.
    /// </summary>
    /// <param name="value">The value of the successful operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    private Result(T value, Dictionary<string, string?>? metadata = null)
    {
        IsSuccess = true;
        _value = value;
        _errors = [];
        Metadata = metadata;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class for a failed operation.
    /// </summary>
    /// <param name="errors">The errors that occurred during the operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    private Result(IEnumerable<IError> errors, Dictionary<string, string?>? metadata = null)
    {
        IsSuccess = false;
        _value = default;
        _errors = [.. errors];
        Metadata = metadata;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value of the successful operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A successful <see cref="Result{T}"/>.</returns>
    public static Result<T> Ok(T value, Dictionary<string, string?>? metadata = null)
    {
        return new Result<T>(value, metadata);
    }

    /// <summary>
    /// Creates a failed result with the specified errors.
    /// </summary>
    /// <param name="errors">The errors that occurred during the operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Ko(IEnumerable<IError> errors, Dictionary<string, string?>? metadata = null)
    {
        if (errors == null || !errors.Any())
        {
            throw new ArgumentException("At least one error must be provided for a failure result.", nameof(errors));
        }
        return new Result<T>(errors, metadata);
    }

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    /// <param name="error">The error that occurred during the operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Ko(IError error, Dictionary<string, string?>? metadata = null)
    {
        return Ko([error], metadata);
    }

    /// <summary>
    /// Creates a failed result with a single error defined by code and message.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Failure(string errorCode, string errorMessage, Dictionary<string, string?>? metadata = null)
    {
        return Ko(new Error(errorCode, errorMessage), metadata);
    }

    /// <summary>
    /// Matches the result to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="TResult">The return type of the match functions.</typeparam>
    /// <param name="onSuccess">Function to execute if the result is successful.</param>
    /// <param name="onFailure">Function to execute if the result is failed.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<IReadOnlyList<IError>, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value) : onFailure(Errors);
    }

    /// <summary>
    /// Executes one of two actions based on success or failure.
    /// </summary>
    /// <param name="onSuccess">Action to execute if the result is successful.</param>
    /// <param name="onFailure">Action to execute if the result is failed.</param>
    public void Match(
        Action<T> onSuccess,
        Action<IReadOnlyList<IError>> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess(Value);
        }
        else
        {
            onFailure(Errors);
        }
    }
}

/// <summary>
/// Provides non-generic result functionality for operations that don't return a value.
/// </summary>
public class Result
{
    private readonly List<IError> _errors;

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the collection of errors if the operation failed.
    /// </summary>
    public IReadOnlyList<IError> Errors => _errors.AsReadOnly();

    /// <summary>
    /// Gets or sets optional metadata associated with this result.
    /// </summary>
    public Dictionary<string, string?>? Metadata { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class for a successful operation.
    /// </summary>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    private Result(Dictionary<string, string?>? metadata = null)
    {
        IsSuccess = true;
        _errors = [];
        Metadata = metadata;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class for a failed operation.
    /// </summary>
    /// <param name="errors">The errors that occurred during the operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    private Result(IEnumerable<IError> errors, Dictionary<string, string?>? metadata = null)
    {
        IsSuccess = false;
        _errors = [.. errors];
        Metadata = metadata;
    }

    /// <summary>
    /// Creates a successful result without a value.
    /// </summary>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Ok(Dictionary<string, string?>? metadata = null)
    {
        return new Result(metadata);
    }

    /// <summary>
    /// Creates a failed result with the specified errors.
    /// </summary>
    /// <param name="errors">The errors that occurred during the operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Ko(IEnumerable<IError> errors, Dictionary<string, string?>? metadata = null)
    {
        if (errors == null || !errors.Any())
        {
            throw new ArgumentException("At least one error must be provided for a failure result.", nameof(errors));
        }
        return new Result(errors, metadata);
    }

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    /// <param name="error">The error that occurred during the operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Ko(IError error, Dictionary<string, string?>? metadata = null)
    {
        return Ko([error], metadata);
    }

    /// <summary>
    /// Creates a failed result with a single error defined by code and message.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Ko(string errorCode, string errorMessage, Dictionary<string, string?>? metadata = null)
    {
        return Ko(new Error(errorCode, errorMessage), metadata);
    }
}
