# Result Object Documentation

The `Result<T>` object is a generic type that provides a robust way to handle operations that can either succeed with a value or fail with errors. It includes support for metadata at both the result and error levels.

## Core Components

### Result<T>
The main generic result type that can contain either a success value or a collection of errors.

**Properties:**
- `IsSuccess`: Boolean indicating if the operation succeeded
- `IsFailure`: Boolean indicating if the operation failed
- `Value`: The success value (throws if accessed on failure)
- `Errors`: Read-only list of errors (empty for success)
- `Metadata`: Optional dictionary for attaching metadata to the result

### ResultError
Represents an error with structured information.

**Properties:**
- `Code`: A unique error code (e.g., "VALIDATION_ERROR")
- `Message`: Human-readable error description
- `Metadata`: Optional dictionary for error-specific metadata

### ResultFactory
Factory class providing convenient methods for creating results.

## Usage Examples

### 1. Basic Success Result
```csharp
var result = ResultFactory.Success(42);

if (result.IsSuccess)
{
    Console.WriteLine($"Value: {result.Value}");
}
```

### 2. Success with Metadata
```csharp
var result = ResultFactory.Success(
    new User { Id = 1, Name = "John" },
    new Dictionary<string, string?> 
    { 
        { "timestamp", DateTime.UtcNow.ToString() },
        { "source", "database" }
    }
);
```

### 3. Simple Failure
```csharp
var result = ResultFactory.Failure<User>(
    ErrorCodes.NotFound,
    "User not found"
);
```

### 4. Failure with Metadata
```csharp
var result = ResultFactory.Failure<User>(
    ErrorCodes.ValidationError,
    "Invalid email format",
    new Dictionary<string, string?> 
    { 
        { "field", "email" },
        { "attemptedValue", "invalid@" }
    }
);
```

### 5. Error with Metadata
```csharp
var error = ResultFactory.CreateError(
    ErrorCodes.NotFound,
    "User not found",
    new Dictionary<string, string?> 
    { 
        { "userId", "123" },
        { "searchedAt", DateTime.UtcNow.ToString() }
    }
);

var result = ResultFactory.Failure<User>(error);
```

### 6. Multiple Errors
```csharp
var errors = new[]
{
    ResultFactory.CreateError(
        ErrorCodes.ValidationError, 
        "Email is required",
        new Dictionary<string, string?> { { "field", "email" } }
    ),
    ResultFactory.CreateError(
        ErrorCodes.ValidationError,
        "Password must be at least 8 characters",
        new Dictionary<string, string?> { { "field", "password" } }
    )
};

var result = ResultFactory.Failure<User>(errors);
```

### 7. Using Match for Pattern Matching
```csharp
var result = GetUser(id);

var response = result.Match(
    onSuccess: user => $"Found user: {user.Name}",
    onFailure: errors => $"Error: {errors.First().Message}"
);
```

### 8. Using Match with Actions
```csharp
result.Match(
    onSuccess: user => Console.WriteLine($"User: {user.Name}"),
    onFailure: errors => 
    {
        foreach (var error in errors)
        {
            Console.WriteLine($"[{error.Code}] {error.Message}");
        }
    }
);
```

### 9. Static Factory Methods on Result<T>
```csharp
// Direct usage without ResultFactory
var successResult = Result<int>.Success(42);

var failureResult = Result<int>.Failure(
    "ERROR_CODE",
    "Something went wrong"
);
```

### 10. Adding Metadata After Creation
```csharp
var result = ResultFactory.Success(data);
result.Metadata ??= new Dictionary<string, string?>();
result.Metadata["processingTime"] = "150ms";
```

## Pre-defined Error Codes

The `ErrorCodes` class provides common error codes:

- `ValidationError`: "VALIDATION_ERROR"
- `NotFound`: "NOT_FOUND"
- `Unauthorized`: "UNAUTHORIZED"
- `InternalError`: "INTERNAL_ERROR"
- `InvalidOperation`: "INVALID_OPERATION"
- `Conflict`: "CONFLICT"

You can also use custom error codes as needed.

## Best Practices

1. **Always check `IsSuccess` before accessing `Value`** - accessing `Value` on a failed result throws an exception
2. **Use meaningful error codes** - they help with client-side error handling
3. **Add context with metadata** - both result-level and error-level metadata help with debugging
4. **Use `Match` for clean handling** - it forces you to handle both success and failure cases
5. **Prefer specific error codes** - use the pre-defined codes when applicable, or create domain-specific ones

## Web API Integration

The Result object works seamlessly with ASP.NET Core minimal APIs:

```csharp
app.MapGet("/users/{id}", (int id) =>
{
    if (id <= 0)
    {
        return ResultFactory.Failure<User>(
            ErrorCodes.ValidationError,
            "ID must be greater than zero"
        );
    }
    
    var user = userRepository.GetById(id);
    if (user == null)
    {
        return ResultFactory.Failure<User>(
            ErrorCodes.NotFound,
            $"User with ID {id} not found"
        );
    }
    
    return ResultFactory.Success(user);
});
```

## JSON Serialization

The Result object serializes cleanly to JSON with all properties:

**Success:**
```json
{
  "isSuccess": true,
  "isFailure": false,
  "value": { "id": 1, "name": "John" },
  "errors": [],
  "metadata": { "timestamp": "2024-01-15T10:30:00Z" }
}
```

**Failure:**
```json
{
  "isSuccess": false,
  "isFailure": true,
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "Email is required",
      "metadata": { "field": "email" }
    }
  ],
  "metadata": { "requestId": "abc-123" }
}
```
