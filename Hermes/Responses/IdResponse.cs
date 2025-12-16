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
public record IdResponse<T>: Response<IdResponseData<T>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdResponse{T}"/> class.
    /// </summary>
    /// <param name="data">The ID response data.</param>
    /// <param name="attributes">Optional metadata attributes.</param>
    public IdResponse(IdResponseData<T> data, Dictionary<string, string?> attributes) : base(data, attributes) 
    {
        
    }
}
