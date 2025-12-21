using System.Text.Json.Serialization;

namespace Hermes.Results;

/// <summary>
/// Provides non-generic result functionality for operations that don't return a value.
/// Serves as the base class for Result&lt;T&gt;.
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

    #region Utility

    /// <summary>
    /// Validates that errors are provided for a failed result.
    /// </summary>
    /// <param name="errors">The errors to validate.</param>
    /// <exception cref="ArgumentException">Thrown when no errors are provided for a failure result.</exception>
    protected static void ValidateErrors(IEnumerable<IError> errors)
    {
        if (errors == null || !errors.Any())
        {
            throw new ArgumentException("At least one error must be provided for a failure result.", nameof(errors));
        }
    }

    /// <summary>
    /// Matches the result to one of two functions based on success or failure.
    /// </summary>
    /// <typeparam name="TResult">The return type of the match functions.</typeparam>
    /// <param name="onSuccess">Function to execute if the result is successful.</param>
    /// <param name="onFailure">Function to execute if the result is failed.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<IReadOnlyList<IError>, TResult> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(Errors);
    }

    /// <summary>
    /// Executes one of two actions based on success or failure.
    /// </summary>
    /// <param name="onSuccess">Action to execute if the result is successful.</param>
    /// <param name="onFailure">Action to execute if the result is failed.</param>
    public void Match(
        Action onSuccess,
        Action<IReadOnlyList<IError>> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess();
        }
        else
        {
            onFailure(Errors);
        }
    }

    /// <summary>
    /// Implicitly converts an Error to a failed Result.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    public static implicit operator Result(Error error)
    {
        return Ko(error);
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Whether the operation was successful.</param>
    /// <param name="errors">The errors that occurred during the operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    protected Result(bool isSuccess, IEnumerable<IError>? errors, Dictionary<string, string?>? metadata)
    {
        IsSuccess = isSuccess;
        _errors = errors != null ? [.. errors] : [];
        Metadata = metadata;
    }

    #endregion

    #region Factory Methods - Non-Generic Result

    /// <summary>
    /// Creates a successful result without a value.
    /// </summary>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Ok(Dictionary<string, string?>? metadata = null)
    {
        return new Result(true, null, metadata);
    }

    /// <summary>
    /// Creates a failed result with the specified errors.
    /// </summary>
    /// <param name="errors">The errors that occurred during the operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Ko(IEnumerable<IError> errors, Dictionary<string, string?>? metadata = null)
    {
        ValidateErrors(errors);
        return new Result(false, errors, metadata);
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

    #endregion

    // These methods are here to allow inference of T when calling Result.Ok(...) or Result.Ko(...)
    // without having to specify the type parameter explicitly.
    // For example: var result = Result.Ok(42); instead of var result = Result<int>.Ok(42);
    #region Factory Methods - Generic Result

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value of the successful operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A successful <see cref="Result{T}"/>.</returns>
    public static Result<T> Ok<T>(T value, Dictionary<string, string?>? metadata = null)
    {
        return Result<T>.Ok(value, metadata);
    }

    /// <summary>
    /// Creates a failed result with the specified errors.
    /// </summary>
    /// <param name="errors">The errors that occurred during the operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Ko<T>(IEnumerable<IError> errors, Dictionary<string, string?>? metadata = null)
    {
        ValidateErrors(errors);
        return Result<T>.Ko(errors, metadata);
    }

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    /// <param name="error">The error that occurred during the operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Ko<T>(IError error, Dictionary<string, string?>? metadata = null)
    {
        return Result<T>.Ko([error], metadata);
    }

    /// <summary>
    /// Creates a failed result with a single error defined by code and message.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Ko<T>(string errorCode, string errorMessage, Dictionary<string, string?>? metadata = null)
    {
        return Result<T>.Ko(new Error(errorCode, errorMessage), metadata);
    }

    #endregion
}

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail with errors.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public class Result<T> : Result
{
    private readonly T? _value;

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

    #region Utility

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

    /// <summary>
    /// Implicitly converts a value to a successful Result.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator Result<T>(T value)
    {
        return Ok(value);
    }

    /// <summary>
    /// Implicitly converts a Result to its value. Throws if the result is failed.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <exception cref="InvalidOperationException">Thrown when attempting to convert a failed result to a value.</exception>
    public static implicit operator T(Result<T> result)
    {
        return result.Value;
    }

    /// <summary>
    /// Implicitly converts an Error to a failed Result.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    public static implicit operator Result<T>(Error error)
    {
        return Ko(error);
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class for a successful operation.
    /// </summary>
    /// <param name="value">The value of the successful operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    protected Result(T value, Dictionary<string, string?>? metadata = null)
        : base(true, null, metadata)
    {
        _value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class for a failed operation.
    /// </summary>
    /// <param name="errors">The errors that occurred during the operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    protected Result(IEnumerable<IError> errors, Dictionary<string, string?>? metadata = null)
        : base(false, errors, metadata)
    {
        _value = default;
    }

    #endregion

    #region Factory Methods

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
    public static new Result<T> Ko(IEnumerable<IError> errors, Dictionary<string, string?>? metadata = null)
    {
        ValidateErrors(errors);
        return new Result<T>(errors, metadata);
    }

    /// <summary>
    /// Creates a failed result with a single error.
    /// </summary>
    /// <param name="error">The error that occurred during the operation.</param>
    /// <param name="metadata">Optional metadata to associate with the result.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static new Result<T> Ko(IError error, Dictionary<string, string?>? metadata = null)
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
    public static new Result<T> Ko(string errorCode, string errorMessage, Dictionary<string, string?>? metadata = null)
    {
        return Ko(new Error(errorCode, errorMessage), metadata);
    }

    #endregion
}
