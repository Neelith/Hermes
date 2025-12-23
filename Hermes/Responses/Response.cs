namespace Hermes.Responses;

/// <summary>
/// Represents a generic response wrapper containing data and optional attributes.
/// </summary>
/// <typeparam name="T">The type of the data being returned.</typeparam>
/// <param name="Data">The main data payload of the response.</param>
/// <param name="Attributes">Optional metadata attributes associated with the response.</param>
public record Response<T>(T Data, Dictionary<string, string?>? Attributes) : IResponse
{
    /// <summary>
    /// Creates a generic response with the specified data and optional attributes.
    /// </summary>
    /// <param name="data">The data to include in the response.</param>
    /// <param name="attributes">Optional metadata attributes. If null, an empty dictionary is used.</param>
    /// <returns>A new <see cref="Response{T}"/> instance.</returns>
    public static Response<T> Create(T data, Dictionary<string, string?>? attributes = null)
    {
        return new Response<T>(data, attributes);
    }
}
