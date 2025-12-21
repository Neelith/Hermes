# Result Object Documentation

The `Result<T>` object is a generic type that provides a robust way to handle operations that can either succeed with a value or fail with errors. It includes support for metadata at both the result and error levels, and features implicit conversion operators for seamless integration.

## Core Components

### Result (Base Class)
Non-generic result for operations that don't return a value. Serves as the base class for `Result<T>`.

**Properties:**
- `IsSuccess`: Boolean indicating if the operation succeeded
- `IsFailure`: Boolean indicating if the operation failed
- `Errors`: Read-only list of errors (empty for success)
- `Metadata`: Optional dictionary for attaching metadata to the result

**Factory Methods:**
- `Result.Ok(metadata?)`: Create a successful result
- `Result.Ko(errors, metadata?)`: Create a failed result with multiple errors
- `Result.Ko(error, metadata?)`: Create a failed result with single error
- `Result.Ko(errorCode, errorMessage, metadata?)`: Create a failed result with code and message

### Result<T>
The main generic result type that can contain either a success value or a collection of errors.

**Additional Properties:**
- `Value`: The success value (throws if accessed on failure)

**Factory Methods:**
- `Result<T>.Ok(value, metadata?)`: Create a successful result with value
- `Result<T>.Ko(errors, metadata?)`: Create a failed result with multiple errors
- `Result<T>.Ko(error, metadata?)`: Create a failed result with single error
- `Result<T>.Ko(errorCode, errorMessage, metadata?)`: Create a failed result with code and message

### IError / Error
Represents an error with structured information.

**Properties:**
- `Code`: A unique error code (e.g., "VALIDATION_ERROR")
- `Message`: Human-readable error description
- `Metadata`: Optional dictionary for error-specific metadata

## Implicit Conversion Operators

The Result object supports three implicit conversions:

1. **Value to Result**: Automatically wrap a value in a successful result
2. **Result to Value**: Automatically extract the value from a successful result
3. **Error to Result**: Automatically create a failed result from an error

## Usage Examples

### 1. Basic Success Result
```csharp
// Using static factory method
var result = Result<int>.Ok(42);

// Using implicit conversion from value
Result<int> result = 42;

if (result.IsSuccess)
{
    Console.WriteLine($"Value: {result.Value}");
}
```

### 2. Success with Metadata
```csharp
var result = Result<User>.Ok(
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
var result = Result<User>.Ko(
    ErrorCodes.NotFound,
    "User not found"
);
```

### 4. Failure with Metadata
```csharp
var result = Result<User>.Ko(
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
var error = new Error(
    ErrorCodes.NotFound,
    "User not found",
    new Dictionary<string, string?> 
    { 
        { "userId", "123" },
        { "searchedAt", DateTime.UtcNow.ToString() }
    }
);

var result = Result<User>.Ko(error);

// Or using implicit conversion
Result<User> result = error;
```

### 6. Multiple Errors
```csharp
var errors = new[]
{
    new Error(
        ErrorCodes.ValidationError, 
        "Email is required",
        new Dictionary<string, string?> { { "field", "email" } }
    ),
    new Error(
        ErrorCodes.ValidationError,
        "Password must be at least 8 characters",
        new Dictionary<string, string?> { { "field", "password" } }
    )
};

var result = Result<User>.Ko(errors);
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

### 9. Implicit Value Extraction
```csharp
Result<string> result = Result<string>.Ok("Hello, World!");

// Implicit conversion from Result<string> to string
string value = result;

Console.WriteLine(value); // "Hello, World!"

// Note: This will throw InvalidOperationException if result is failed
```

### 10. Implicit Value Wrapping
```csharp
public Result<int> Calculate(int a, int b)
{
    // Implicit conversion from int to Result<int>
    return a + b;
}

var result = Calculate(5, 3);
Console.WriteLine(result.Value); // 8
```

### 11. Non-Generic Result
```csharp
// For operations that don't return a value
Result result = Result.Ok();

if (result.IsSuccess)
{
    Console.WriteLine("Operation completed successfully");
}

// Failure case
Result failureResult = Result.Ko(
    ErrorCodes.InvalidOperation,
    "Cannot perform this operation"
);
```

### 12. Adding Metadata After Creation
```csharp
var result = Result<string>.Ok(data);
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

## API Naming Convention

The Result API uses **Ok/Ko** naming (from HTTP status concepts):
- **Ok**: Success (HTTP 2xx)
- **Ko**: Failure/Error (HTTP 4xx/5xx)

This provides:
- Short, memorable method names
- Clear success/failure semantics
- Consistent API across Result and Result<T>

## Best Practices

1. **Always check `IsSuccess` before accessing `Value`** - accessing `Value` on a failed result throws an exception (unless using implicit conversion in a try-catch)
2. **Use implicit conversions for cleaner code** - they make the API more intuitive
3. **Use meaningful error codes** - they help with client-side error handling
4. **Add context with metadata** - both result-level and error-level metadata help with debugging
5. **Use `Match` for clean handling** - it forces you to handle both success and failure cases
6. **Prefer specific error codes** - use the pre-defined codes when applicable, or create domain-specific ones
7. **Use non-generic Result for void operations** - for operations that don't return data
8. **Use static factory methods directly** - no need for a factory class, call `Result.Ok()` or `Result<T>.Ok()` directly

## Web API Integration

The Result object works seamlessly with ASP.NET Core minimal APIs:

```csharp
app.MapGet("/users/{id}", (int id) =>
{
    if (id <= 0)
    {
        return Result<User>.Ko(
            ErrorCodes.ValidationError,
            "ID must be greater than zero"
        );
    }
    
    var user = userRepository.GetById(id);
    if (user == null)
    {
        return Result<User>.Ko(
            ErrorCodes.NotFound,
            $"User with ID {id} not found"
        );
    }
    
    // Implicit conversion from User to Result<User>
    return user;
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

## Architecture Benefits

### Simple Design
- No factory class needed - use static factory methods directly
- `Result<T>` inherits from `Result` for natural type hierarchy
- Clean inheritance chain with shared functionality

### Type Safety
- Generic `Result<T>` ensures type safety for success values
- Compile-time checking of value types
- No casting required

### Flexibility
- Implicit conversions reduce boilerplate
- Support for single or multiple errors
- Extensible metadata at both result and error levels

## Migration from ResultFactory

If you previously used `ResultFactory`, simply replace with direct static method calls:

```csharp
// Before
var result = ResultFactory.Ok(value);
var failure = ResultFactory.Ko<User>(code, message);

// After
var result = Result<string>.Ok(value);
var failure = Result<User>.Ko(code, message);
```

The API is identical except you call the static methods directly on `Result` or `Result<T>`.
