namespace Hermes.Responses;

/// <summary>
/// Provides factory methods for creating standardized response objects.
/// </summary>
public static class ResponseFactory
{
    /// <summary>
    /// Creates a generic response with the specified data and optional attributes.
    /// </summary>
    /// <typeparam name="T">The type of the data being returned.</typeparam>
    /// <param name="data">The data to include in the response.</param>
    /// <param name="attributes">Optional metadata attributes. If null, an empty dictionary is used.</param>
    /// <returns>A new <see cref="Response{T}"/> instance.</returns>
    public static Response<T> CreateResponse<T>(T data, Dictionary<string, string?>? attributes = null)
    {
        return new Response<T>(data, attributes);
    }

    /// <summary>
    /// Creates an ID-based response with the specified identifier and optional attributes.
    /// </summary>
    /// <typeparam name="T">The type of the identifier.</typeparam>
    /// <param name="id">The identifier value to include in the response.</param>
    /// <param name="attributes">Optional metadata attributes. If null, an empty dictionary is used. The Type attribute is automatically added.</param>
    /// <returns>A new <see cref="IdResponse{T}"/> instance with the 'type' attribute set to the type name of T.</returns>
    public static IdResponse<T> CreateIdResponse<T>(T id, Dictionary<string, string?>? attributes = null)
    {
        attributes ??= [];

        attributes[ResponseAttributes.Type] = typeof(T).Name;

        var data = new IdResponseData<T>(id);

        return new IdResponse<T>(data, attributes);
    }

    /// <summary>
    /// Creates a paged response with the specified items, total count, and optional attributes.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="items">The collection of items for the current page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <param name="attributes">Optional metadata attributes. If null, an empty dictionary is used. The TotalCount attribute is automatically added.</param>
    /// <returns>A new <see cref="PagedResponse{T}"/> instance with the 'totalCount' attribute set.</returns>
    public static PagedResponse<T> CreatePagedResponse<T>(IEnumerable<T> items, int totalCount, Dictionary<string, string?>? attributes = null)
    {
        attributes ??= [];

        attributes[ResponseAttributes.TotalCount] = totalCount.ToString();

        var data = new PagedResponseData<T>(items);

        return new PagedResponse<T>(data, attributes);
    }
}
