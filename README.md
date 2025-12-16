# Hermes

> *The messenger of the gods brings consistency to your API responses.*

## Why Hermes?

### The Problem

Modern APIs face several challenges when returning data to clients:

1. **Inconsistent Response Formats**: Different endpoints return data in different structures, forcing clients to handle multiple response patterns
2. **Missing Metadata**: Pagination information, resource types, and other metadata are often embedded inconsistently or omitted entirely
3. **Client Complexity**: Clients must write different deserialization logic for each endpoint format
4. **Maintenance Burden**: As APIs evolve, maintaining consistent response structures across all endpoints becomes increasingly difficult

### The Solution

Hermes provides a standardized envelope pattern for ASP.NET Core APIs. By wrapping your responses in consistent, well-defined structures, you get:

- **Predictable API Contracts**: All responses follow the same pattern, making client integration trivial
- **Extensible Metadata**: Attach arbitrary attributes to any response without breaking the contract
- **Type Safety**: Generic types ensure compile-time correctness for both data and metadata
- **Simplified Consumption**: Clients can deserialize any response using the same logic

## What is Hermes?

Hermes is a lightweight .NET library that provides standardized response types for building consistent REST APIs.

### Response Types

#### `Response<T>` - Generic Response Wrapper

The foundation of all responses, wrapping any data type with optional metadata attributes.

```csharp
var user = GetUser(id);
var response = ResponseFactory.CreateResponse(user, new Dictionary<string, string?>
{
    { "cached", "true" },
    { "expires", "2024-12-31" }
});
```

**Output:**
```json
{
  "data": {
    "id": 1,
    "name": "John Doe"
  },
  "attributes": {
    "cached": "true",
    "expires": "2024-12-31"
  }
}
```

#### `IdResponse<T>` - Identifier Response

Specialized for returning IDs after creation or update operations, automatically including type information.

```csharp
var orderId = CreateOrder(request);
var response = ResponseFactory.CreateIdResponse(orderId);
```

**Output:**
```json
{
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
  },
  "attributes": {
    "type": "Guid"
  }
}
```

#### `PagedResponse<T>` - Paginated Collections

Designed for collections, automatically including total count and supporting custom pagination metadata.

```csharp
var products = GetProducts(page, pageSize);
var totalCount = GetTotalProductCount();

var response = ResponseFactory.CreatePagedResponse(products, totalCount, new Dictionary<string, string?>
{
    { "page", page.ToString() },
    { "pageSize", pageSize.ToString() }
});
```

**Output:**
```json
{
  "data": {
    "items": [
      { "id": 1, "name": "Product A" },
      { "id": 2, "name": "Product B" }
    ]
  },
  "attributes": {
    "totalCount": "150",
    "page": "1",
    "pageSize": "10"
  }
}
```

## Features

✅ **Consistent Envelope Pattern**: All responses follow the same `{ data, attributes }` structure  
✅ **Type-Safe Generics**: Full compile-time type checking for data and metadata  
✅ **Factory Methods**: Simple, fluent API for creating responses  
✅ **Extensible Attributes**: Add custom metadata without breaking contracts  
✅ **Zero Dependencies**: Pure .NET 10 with no external packages  
✅ **Record Types**: Immutable responses with built-in structural equality  

## Installation

```bash
dotnet add package Neelith.Hermes
```

## Quick Start

```csharp
using Hermes.Responses;

// In your ASP.NET Core endpoints or controllers

// Simple response
var data = new { message = "Hello, World!" };
return ResponseFactory.CreateResponse(data);

// ID response after creation
var newId = Guid.NewGuid();
return ResponseFactory.CreateIdResponse(newId);

// Paged collection
var items = new[] { "Item 1", "Item 2", "Item 3" };
return ResponseFactory.CreatePagedResponse(items, items.Length);
```

## Response Attributes

Hermes includes predefined attribute constants in `ResponseAttributes`:

- `ResponseAttributes.TotalCount` - `"totalCount"` for pagination
- `ResponseAttributes.Type` - `"type"` for resource type information

You can use these constants or add your own custom attributes:

```csharp
var attributes = new Dictionary<string, string?>
{
    { ResponseAttributes.TotalCount, "100" },
    { "cursor", "eyJpZCI6MTAwfQ==" },
    { "hasMore", "true" }
};
```

## Advanced Usage

### Custom Attributes

Add custom metadata to any response:

```csharp
var customAttributes = new Dictionary<string, string?>
{
    { "version", "2.0" },
    { "requestId", context.TraceIdentifier },
    { "deprecated", "false" }
};

return ResponseFactory.CreateResponse(data, customAttributes);
```

### Complex Data Structures

Hermes works seamlessly with nested objects and collections:

```csharp
var complexData = new
{
    user = new { id = 1, name = "Alice" },
    permissions = new[] { "read", "write" },
    metadata = new { lastLogin = DateTime.UtcNow }
};

return ResponseFactory.CreateResponse(complexData);
```

### Integration with Controllers

Use Hermes in your MVC controllers:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProducts(int page = 1, int pageSize = 10)
    {
        var products = _productService.GetProducts(page, pageSize);
        var totalCount = _productService.GetTotalCount();
        
        var response = ResponseFactory.CreatePagedResponse(products, totalCount);
        return Ok(response);
    }

    [HttpPost]
    public IActionResult CreateProduct(CreateProductRequest request)
    {
        var productId = _productService.Create(request);
        var response = ResponseFactory.CreateIdResponse(productId);
        return CreatedAtAction(nameof(GetProduct), new { id = productId }, response);
    }
}
```

## Why "Hermes"?

In Greek mythology, Hermes was the messenger of the gods, known for delivering messages reliably and consistently. Similarly, this library ensures your API messages (responses) are delivered in a consistent, reliable format to your clients.

## Requirements

- .NET 10.0 or later

## License

This project is licensed under the terms specified in the LICENSE file.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## Support

For issues, questions, or contributions, visit the [GitHub repository](https://github.com/Neelith/Hermes).

## Author

**Neelith**

---

*Deliver your API responses with divine consistency.* ⚡
