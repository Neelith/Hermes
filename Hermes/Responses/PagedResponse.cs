namespace YourProjectName.Shared.Responses;

/// <summary>
/// Represents the data structure for a paged collection response.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
/// <param name="Items">The collection of items for the current page.</param>
public record PagedResponseData<T>(IEnumerable<T> Items);

/// <summary>
/// Represents a specialized response for returning paged collections of data.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
public record PagedResponse<T> : Response<PagedResponseData<T>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PagedResponse{T}"/> class.
    /// </summary>
    /// <param name="data">The paged response data containing the items.</param>
    /// <param name="attributes">Optional metadata attributes, typically including pagination information.</param>
    public PagedResponse(PagedResponseData<T> data, Dictionary<string, string?> attributes) : base(data, attributes) 
    {
        
    }
}
