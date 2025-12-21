namespace Hermes.Responses;

/// <summary>
/// Represents a generic response wrapper containing data and optional attributes.
/// </summary>
/// <typeparam name="T">The type of the data being returned.</typeparam>
/// <param name="Data">The main data payload of the response.</param>
/// <param name="Attributes">Optional metadata attributes associated with the response.</param>
public record Response<T>(T Data, Dictionary<string, string?>? Attributes);
