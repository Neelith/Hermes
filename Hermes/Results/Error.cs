namespace Hermes.Results;

/// <summary>
/// Represents an error that occurred during an operation.
/// </summary>
/// <param name="Code">A unique code identifying the type of error.</param>
/// <param name="Message">A human-readable description of the error.</param>
/// <param name="Metadata">Optional additional metadata associated with the error.</param>
public record Error(
    string Code,
    string Message,
    Dictionary<string, string?>? Metadata = null) : IError;
