namespace Hermes.Responses;

/// <summary>
/// Represents the data structure for an ID-based response.
/// </summary>
/// <typeparam name="T">The type of the identifier.</typeparam>
/// <param name="Id">The identifier value.</param>
public record IdResponseData<T>(T Id);


/// <summary>
/// Represents a specialized response for returning ID values.
/// </summary>
/// <typeparam name="T">The type of the identifier.</typeparam>
public record IdResponse<T> : Response<IdResponseData<T>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdResponse{T}"/> class.
    /// </summary>
    /// <param name="data">The ID response data.</param>
    /// <param name="attributes">Optional metadata attributes.</param>
    public IdResponse(IdResponseData<T> data, Dictionary<string, string?> attributes) : base(data, attributes)
    {

    }

    /// <summary>
    /// Creates an ID-based response with the specified identifier and optional attributes.
    /// </summary>
    /// <param name="id">The identifier value to include in the response.</param>
    /// <param name="attributes">Optional metadata attributes. If null, an empty dictionary is used. The Type attribute is automatically added.</param>
    /// <returns>A new <see cref="IdResponse{T}"/> instance with the 'type' attribute set to the type name of T.</returns>
    public static IdResponse<T> Create(T id, Dictionary<string, string?>? attributes = null)
    {
        attributes ??= [];

        attributes[ResponseAttributes.Type] = typeof(T).Name;

        var data = new IdResponseData<T>(id);

        return new IdResponse<T>(data, attributes);
    }
}
