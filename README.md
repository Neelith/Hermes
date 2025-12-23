# Hermes

> *The messenger of the gods brings consistency to your API responses.*

## Why Hermes?

### The Problem

Modern APIs face several challenges when returning data to clients:

1. **Inconsistent Response Formats**: Different endpoints return data in different structures, forcing clients to handle multiple response patterns
2. **Missing Metadata**: Pagination information, resource types, and other metadata are often embedded inconsistently or omitted entirely
3. **Client Complexity**: Clients must write different deserialization logic for each endpoint format
4. **Maintenance Burden**: As APIs evolve, maintaining consistent response structures across all endpoints becomes increasingly difficult
5. **Error Handling**: Exception-based error handling can be unpredictable and makes it difficult to represent multiple validation errors

### The Solution

Hermes provides two complementary patterns for ASP.NET Core APIs:

**Response Envelope Pattern**: Wrapping your responses in consistent, well-defined structures, you get:
- **Predictable API Contracts**: All responses follow the same pattern, making client integration trivial
- **Extensible Metadata**: Attach arbitrary attributes to any response without breaking the contract
- **Type Safety**: Generic types ensure compile-time correctness for both data and metadata
- **Simplified Consumption**: Clients can deserialize any response using the same logic

**Result Pattern**: Explicit success/failure handling for operations, providing:
- **Railway-Oriented Programming**: Chain operations that can fail without exception handling
- **Multiple Error Support**: Collect and return multiple validation errors in a single response
- **Type-Safe Error Handling**: Errors are strongly-typed and always available for inspection
- **Functional Composition**: Transform and compose operations using Map, OnSuccess, and OnFailure

## What is Hermes?

Hermes is a lightweight .NET library that provides standardized response types and result patterns for building consistent REST APIs.

## Features

### Response Envelope

✅ **Consistent Envelope Pattern**: All responses follow the same `{ data, attributes }` structure  
✅ **Type-Safe Generics**: Full compile-time type checking for data and metadata  
✅ **Factory Methods**: Simple, fluent API for creating responses  
✅ **Extensible Attributes**: Add custom metadata without breaking contracts  

### Result Pattern

✅ **Explicit Success/Failure**: No exceptions for business logic failures  
✅ **Multiple Errors**: Collect and return multiple validation errors  
✅ **Functional Extensions**: Map, OnSuccess, OnFailure for chaining operations  
✅ **Pattern Matching**: Match method for handling both success and failure cases  
✅ **Implicit Conversions**: Natural syntax for creating results  

### Common

✅ **Zero Dependencies**: Pure .NET 10 with no external packages  
✅ **Record Types**: Immutable responses with built-in structural equality  

## Installation

```bash
dotnet add package Neelith.Hermes
```

## Quick Start

### Response Envelope

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

### Result Pattern

```csharp
using Hermes.Results;

// Operation that can fail
public Result<User> GetUser(int id)
{
    var user = _repository.FindById(id);
    if (user == null)
        return Result.Ko<User>("USER_NOT_FOUND", "User not found");
    
    return Result.Ok(user);
}

// Chaining operations
var result = GetUser(id)
    .Map(user => new UserDto(user))
    .OnSuccess(dto => _logger.LogInfo($"Retrieved user {dto.Name}"))
    .OnFailure(errors => _logger.LogError($"Failed: {errors[0].Message}"));
```

## Response Types

### `Response<T>` - Generic Response Wrapper

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

### `IdResponse<T>` - Identifier Response

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

### `PagedResponse<T>` - Paginated Collections

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

## Result Pattern

The Result pattern provides a functional approach to error handling, allowing you to represent operations that can succeed or fail without throwing exceptions.

### `Result` - Non-Generic Result

For operations that don't return a value but can succeed or fail.

```csharp
public Result DeleteUser(int id)
{
    var user = _repository.FindById(id);
    if (user == null)
        return Result.Ko("USER_NOT_FOUND", "User not found");
    
    _repository.Delete(user);
    return Result.Ok();
}
```

### `Result<T>` - Generic Result

For operations that return a value on success or errors on failure.

```csharp
public Result<User> CreateUser(CreateUserRequest request)
{
    // Validate
    if (string.IsNullOrEmpty(request.Email))
        return Result.Ko<User>("INVALID_EMAIL", "Email is required");
    
    if (_repository.ExistsByEmail(request.Email))
        return Result.Ko<User>("DUPLICATE_EMAIL", "Email already exists");
    
    // Create
    var user = new User { Email = request.Email, Name = request.Name };
    _repository.Add(user);
    
    return Result.Ok(user);
}
```

### Multiple Errors

Results can contain multiple errors, perfect for validation scenarios:

```csharp
public Result<User> ValidateUser(CreateUserRequest request)
{
    var errors = new List<IError>();
    
    if (string.IsNullOrEmpty(request.Email))
        errors.Add(new Error("INVALID_EMAIL", "Email is required"));
    
    if (string.IsNullOrEmpty(request.Name))
        errors.Add(new Error("INVALID_NAME", "Name is required"));
    
    if (request.Age < 18)
        errors.Add(new Error("INVALID_AGE", "Must be 18 or older"));
    
    if (errors.Any())
        return Result.Ko<User>(errors);
    
    return Result.Ok(new User { Email = request.Email, Name = request.Name });
}
```

### Error Types

#### `IError` Interface

The base interface for all errors, allowing custom error implementations:

```csharp
public interface IError
{
    string Code { get; init; }
    string Message { get; init; }
    Dictionary<string, string?>? Metadata { get; init; }
}
```

#### `Error` Record

The default implementation of `IError`:

```csharp
var error = new Error(
    Code: "VALIDATION_FAILED",
    Message: "The request validation failed",
    Metadata: new Dictionary<string, string?> 
    { 
        { "field", "email" },
        { "constraint", "unique" }
    }
);
```

### Pattern Matching

Use the `Match` method to handle both success and failure cases:

```csharp
var result = GetUser(id);

// Match with return value
var message = result.Match(
    onSuccess: user => $"Found user: {user.Name}",
    onFailure: errors => $"Error: {errors[0].Message}"
);

// Match with actions
result.Match(
    onSuccess: user => Console.WriteLine($"Success: {user.Name}"),
    onFailure: errors => Console.WriteLine($"Failed: {string.Join(", ", errors.Select(e => e.Message))}")
);
```

### Functional Extensions

#### `Map<TSource, TResult>`

Transform the value inside a successful result:

```csharp
Result<User> userResult = GetUser(id);
Result<UserDto> dtoResult = userResult.Map(user => new UserDto(user));
```

#### `OnSuccess`

Execute an action when the result is successful:

```csharp
result
    .OnSuccess(user => _logger.LogInfo($"Retrieved user {user.Name}"))
    .OnSuccess(user => _cache.Set(user.Id, user));
```

#### `OnFailure`

Execute an action when the result fails:

```csharp
result
    .OnFailure(errors => _logger.LogError($"Operation failed: {errors[0].Message}"))
    .OnFailure(errors => _metrics.IncrementFailureCount());
```

#### Chaining Operations

Combine extensions for clean, readable code:

```csharp
var result = GetUser(id)
    .Map(user => new UserDto(user))
    .OnSuccess(dto => _logger.LogInfo($"Mapped user {dto.Name}"))
    .OnFailure(errors => _logger.LogError($"Failed: {errors[0].Message}"));
```

### Implicit Conversions

Results support implicit conversions for cleaner code:

```csharp
// Value to Result<T>
Result<int> result = 42;

// Error to Result<T>
Result<int> failed = new Error("ERROR", "Something went wrong");

// Result<T> to value (returns default if failed)
Result<int> result = Result.Ok(42);
int value = result; // value = 42
```

### Metadata Support

Both results and errors support optional metadata:

```csharp
var result = Result.Ok(user, new Dictionary<string, string?>
{
    { "source", "cache" },
    { "timestamp", DateTime.UtcNow.ToString("O") }
});

var error = new Error(
    "RATE_LIMIT",
    "Too many requests",
    new Dictionary<string, string?> 
    { 
        { "retryAfter", "60" },
        { "limit", "100" }
    }
);
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

### Combining Result Pattern with Response Envelope

Use both patterns together for comprehensive API design:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult GetUser(int id)
    {
        var result = _userService.GetUser(id);
        
        return result.Match(
            onSuccess: user => 
            {
                var response = ResponseFactory.CreateResponse(user);
                return Ok(response);
            },
            onFailure: errors => 
            {
                var errorResponse = new 
                { 
                    errors = errors.Select(e => new { e.Code, e.Message })
                };
                return NotFound(errorResponse);
            }
        );
    }

    [HttpPost]
    public IActionResult CreateUser(CreateUserRequest request)
    {
        var validationResult = _userService.ValidateUser(request);
        
        if (validationResult.IsFailure)
        {
            return BadRequest(new 
            { 
                errors = validationResult.Errors.Select(e => new { e.Code, e.Message })
            });
        }
        
        var createResult = _userService.CreateUser(request);
        
        return createResult.Match(
            onSuccess: user => 
            {
                var response = ResponseFactory.CreateIdResponse(user.Id);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, response);
            },
            onFailure: errors => 
            {
                return BadRequest(new 
                { 
                    errors = errors.Select(e => new { e.Code, e.Message })
                });
            }
        );
    }
}
```

### Custom Error Types

Implement `IError` for domain-specific error types:

```csharp
public record ValidationError(
    string Code,
    string Message,
    string Field,
    string Constraint,
    Dictionary<string, string?>? Metadata = null
) : IError;

// Usage
var error = new ValidationError(
    Code: "INVALID_EMAIL",
    Message: "Email format is invalid",
    Field: "Email",
    Constraint: "EmailAddress"
);

return Result.Ko<User>(error);
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
