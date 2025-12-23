# Hermes

> *The messenger of the gods brings consistency and structure to your API.*

## Overview

Hermes is a comprehensive .NET library that combines three powerful patterns for building maintainable REST APIs:

1. **CQRS Pattern** - Separate read and write operations with command and query handlers
2. **Response Envelope Pattern** - Wrap responses in consistent, well-defined structures  
3. **Result Pattern** - Explicit success/failure handling without exceptions

## Why Hermes?

### The Problems

Modern APIs face several interconnected challenges:

**Architectural Complexity**
- Business logic scattered across controllers
- Difficult to apply cross-cutting concerns (logging, validation, caching)
- Tight coupling between endpoints and business operations
- Hard to test and maintain as complexity grows

**Inconsistent Response Formats**
- Different endpoints return data in different structures
- Missing metadata like pagination info, resource types
- Clients must handle multiple response patterns
- Difficult to maintain consistent contracts

**Error Handling Chaos**
- Exception-based error handling is unpredictable
- Hard to represent multiple validation errors
- Business failures mixed with technical exceptions
- Inconsistent error responses across endpoints

### The Benefits

Hermes solves these challenges by providing:

✅ **Architectural Clarity** - Clear separation between reads (queries) and writes (commands)  
✅ **Testability** - Each handler is an isolated, testable unit of work  
✅ **Consistency** - All responses follow the same `{ data, attributes }` structure  
✅ **Type Safety** - Full compile-time checking for requests, responses, and errors  
✅ **Explicit Error Handling** - No exceptions for business logic failures  
✅ **Composability** - Chain operations with Map, OnSuccess, and OnFailure  
✅ **Extensibility** - Apply cross-cutting concerns via decorators  
✅ **Scalability** - Optimize read and write paths independently  
✅ **Client Simplicity** - Predictable API contracts make integration trivial  

## Installation

```bash
dotnet add package Neelith.Hermes
```

**Requirements:**
- .NET 10.0 or later
- Microsoft.Extensions.DependencyInjection (included in ASP.NET Core)

**Dependencies:**
- [Scrutor](https://github.com/khellang/Scrutor) (for assembly scanning)

---

### Complete Example

Here's a complete example showing all patterns working together:

```csharp

// 1. Define your DTOs and requests
public record UserDto(int Id, string Name, string Email);
public record CreateUserRequest(string Name, string Email);

// 2. Define your commands and queries
public record GetUserQuery(int UserId) : IQuery<Response<UserDto>>;
public record CreateUserCommand(string Name, string Email) : ICommand<IdResponse<int>>;

// 3. Implement handlers
public class GetUserQueryHandler : IQueryHandler<GetUserQuery, Response<UserDto>>
{
    public async Task<Result<Response<UserDto>>> Handle(
        GetUserQuery query, 
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(query.UserId, cancellationToken);
        
        if (user == null)
            return Result.Ko<Response<UserDto>>("USER_NOT_FOUND", "User not found");
        
        var dto = new UserDto(user.Id, user.Name, user.Email);
        var response = Response<UserDto>.Create(dto);
        
        return Result.Ok(response);
    }
}

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, IdResponse<int>>
{
    public async Task<Result<IdResponse<int>>> Handle(
        CreateUserCommand command, 
        CancellationToken cancellationToken)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(command.Name))
            return Result.Ko<IdResponse<int>>("INVALID_NAME", "Name is required");
            
        if (string.IsNullOrWhiteSpace(command.Email))
            return Result.Ko<IdResponse<int>>("INVALID_EMAIL", "Email is required");
        
        // Business logic
        if (await _repository.EmailExistsAsync(command.Email, cancellationToken))
            return Result.Ko<IdResponse<int>>("DUPLICATE_EMAIL", "Email already exists");
        
        var user = await _repository.CreateAsync(command.Name, command.Email, cancellationToken);
        var response = IdResponse<int>.Create(user.Id);
        
        return Result.Ok(response);
    }
}

// 4. Register handlers in Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHandlers([typeof(Program).Assembly]);

var app = builder.Build();

// 5. Create endpoints
app.MapGet("/users/{id}", async (
    int id,
    IQueryHandler<GetUserQuery, Response<UserDto>> handler,
    CancellationToken ct) =>
{
    var result = await handler.Handle(new GetUserQuery(id), ct);
    
    return result.Match(
        onSuccess: response => Results.Ok(response),
        onFailure: errors => Results.NotFound(new { errors })
    );
});

app.MapPost("/users", async (
    CreateUserRequest request,
    ICommandHandler<CreateUserCommand, IdResponse<int>> handler,
    CancellationToken ct) =>
{
    var command = new CreateUserCommand(request.Name, request.Email);
    var result = await handler.Handle(command, ct);
    
    return result.Match(
        onSuccess: response => Results.Created($"/users/{response.Data.Id}", response),
        onFailure: errors => Results.BadRequest(new { errors })
    );
});

app.Run();
```

**Query Response:**
```json
{
  "data": {
    "id": 1,
    "name": "John Doe",
    "email": "john@example.com"
  },
  "attributes": {}
}
```

**Command Response:**
```json
{
  "data": {
    "id": 42
  },
  "attributes": {
    "type": "Int32"
  }
}
```

**Error Response:**
```json
{
  "errors": [
    {
      "code": "INVALID_EMAIL",
      "message": "Email is required"
    }
  ]
}
```

---

## CQRS & Handlers

Command Query Responsibility Segregation (CQRS) separates read operations (queries) from write operations (commands), allowing you to optimize and scale each path independently.

### Commands vs Queries

**Queries** - Read operations that don't modify state:
- Implement `IQuery<TResponse>`
- Handled by `IQueryHandler<TQuery, TResponse>`
- Should be side-effect free
- Can be cached aggressively

**Commands** - Write operations that modify state:
- Implement `ICommand` or `ICommand<TResponse>`
- Handled by `ICommandHandler<TCommand>` or `ICommandHandler<TCommand, TResponse>`
- Cause side effects (create, update, delete)
- Should validate before executing


### Defining Requests

```csharp
// Query - returns data
public record GetUserQuery(int UserId) : IQuery<Response<UserDto>>;
public record GetUsersQuery(int Page, int PageSize) : IQuery<PagedResponse<UserDto>>;

// Command without response - fire and forget
public record DeleteUserCommand(int UserId) : ICommand;

// Command with response - returns created resource ID
public record CreateUserCommand(string Name, string Email) : ICommand<IdResponse<int>>;
```

### Implementing Handlers

```csharp
// Query handler
public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, PagedResponse<UserDto>>
{
    public async Task<Result<PagedResponse<UserDto>>> Handle(
        GetUsersQuery query, 
        CancellationToken cancellationToken)
    {
        var users = await _repository.GetPageAsync(query.Page, query.PageSize, cancellationToken);
        var totalCount = await _repository.GetTotalCountAsync(cancellationToken);
        
        var response = PagedResponse<UserDto>.Create(
            users.Select(u => new UserDto(u.Id, u.Name, u.Email)),
            totalCount,
            new Dictionary<string, string?>
            {
                { "page", query.Page.ToString() },
                { "pageSize", query.PageSize.ToString() }
            });
        
        return Result.Ok(response);
    }
}

// Command handler (no response)
public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    public async Task<Result> Handle(
        DeleteUserCommand command, 
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(command.UserId, cancellationToken);
        
        if (user == null)
            return Result.Ko("USER_NOT_FOUND", "User not found");
        
        await _repository.DeleteAsync(user, cancellationToken);
        
        return Result.Ok();
    }
}
```

### Handler Registration

Register all handlers automatically by scanning assemblies:

```csharp
// Register handlers from one or more assemblies
builder.Services.AddHandlers([
    typeof(Program).Assembly,
    typeof(GetUserQueryHandler).Assembly
]);
```

The `AddHandlers` method:
- Scans for all `IQueryHandler<,>` implementations
- Scans for all `ICommandHandler<>` and `ICommandHandler<,>` implementations
- Registers them with **Scoped** lifetime
- Supports both public and internal handlers

### Decorators

Apply cross-cutting concerns to all handlers using the decorator pattern:

```csharp
// Logging decorator
public class LoggingQueryHandlerDecorator<TQuery, TResponse> 
    : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : IResponse
{
    public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing query {QueryType}", typeof(TQuery).Name);
        var stopwatch = Stopwatch.StartNew();
        
        var result = await _inner.Handle(query, cancellationToken);
        
        stopwatch.Stop();
        _logger.LogInformation("Query {QueryType} completed in {ElapsedMs}ms", 
            typeof(TQuery).Name, stopwatch.ElapsedMilliseconds);
        
        return result;
    }
}
```

**Common decorator use cases:**
- Logging and metrics
- Validation
- Caching
- Transaction management
- Authorization
- Retry logic
- Performance monitoring

**Decorator execution order** (reverse of registration):
```csharp
services.AddHandlerDecorator(typeof(ICommandHandler<,>), typeof(LoggingDecorator<,>));      // 3rd (outermost)
services.AddHandlerDecorator(typeof(ICommandHandler<,>), typeof(ValidationDecorator<,>));   // 2nd
services.AddHandlerDecorator(typeof(ICommandHandler<,>), typeof(TransactionDecorator<,>)); // 1st (innermost)
// Execution: Transaction -> Validation -> Logging -> Handler
```

---

## Result Pattern

The Result pattern provides functional error handling, allowing operations to return explicit success or failure without throwing exceptions.

### Creating Results

```csharp
// Success
Result<User> result = Result.Ok(user);
Result result = Result.Ok();

// Single error
Result<User> result = Result.Ko<User>("USER_NOT_FOUND", "User not found");

```

### Pattern Matching

Handle success and failure cases explicitly:

```csharp
var result = await handler.Handle(query, cancellationToken);

// Match with return value
return result.Match(
    onSuccess: data => Results.Ok(data),
    onFailure: errors => Results.BadRequest(new { errors })
);

// Match with actions
result.Match(
    onSuccess: data => _logger.LogInformation("Success: {Data}", data),
    onFailure: errors => _logger.LogError("Failed: {Errors}", errors)
);
```

### Functional Composition

Chain operations on results:

```csharp
var result = await GetUser(id)
    .Map(user => new UserDto(user))                                      // Transform success value
    .OnSuccess(dto => _logger.LogInformation("Retrieved {Name}", dto.Name))  // Execute on success
    .OnFailure(errors => _metrics.IncrementFailureCount());              // Execute on failure

return result.Match(
    onSuccess: dto => Results.Ok(Response<UserDto>.Create(dto)),
    onFailure: errors => Results.NotFound(new { errors })
);
```

### Error Types

```csharp
// Default error
var error = new Error(
    Code: "VALIDATION_FAILED",
    Message: "The request validation failed",
    Metadata: new Dictionary<string, string?> 
    { 
        { "field", "email" },
        { "constraint", "format" }
    }
);

// Custom error type
public record ValidationError(
    string Code,
    string Message,
    string Field,
    string Constraint
) : IError
{
    public Dictionary<string, string?>? Metadata => new()
    {
        { "field", Field },
        { "constraint", Constraint }
    };
}
```

### Validation Example

```csharp
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, IdResponse<int>>
{
    public async Task<Result<IdResponse<int>>> Handle(
        CreateUserCommand command, 
        CancellationToken cancellationToken)
    {
        var errors = new List<IError>();
        
        if (string.IsNullOrWhiteSpace(command.Name))
            errors.Add(new Error("INVALID_NAME", "Name is required"));
        
        if (string.IsNullOrWhiteSpace(command.Email))
            errors.Add(new Error("INVALID_EMAIL", "Email is required"));
        else if (!IsValidEmail(command.Email))
            errors.Add(new Error("INVALID_EMAIL_FORMAT", "Email format is invalid"));
        
        if (await _repository.EmailExistsAsync(command.Email, cancellationToken))
            errors.Add(new Error("DUPLICATE_EMAIL", "Email already exists"));
        
        if (errors.Any())
            return Result.Ko<IdResponse<int>>(errors);
        
        var user = await _repository.CreateAsync(command, cancellationToken);
        return Result.Ok(IdResponse<int>.Create(user.Id));
    }
}
```

---

## Response Envelope

The Response Envelope pattern wraps all responses in a consistent `{ data, attributes }` structure, making API contracts predictable and extensible.

### Response Types

**`Response<T>`** - Generic wrapper for any data:

```csharp
var user = new UserDto(1, "John Doe", "john@example.com");
var response = Response<UserDto>.Create(user, new Dictionary<string, string?>
{
    { "cached", "true" },
    { "version", "2.0" }
});
```

```json
{
  "data": {
    "id": 1,
    "name": "John Doe",
    "email": "john@example.com"
  },
  "attributes": {
    "cached": "true",
    "version": "2.0"
  }
}
```

**`IdResponse<T>`** - For returning IDs after creation:

```csharp
var response = IdResponse<int>.Create(userId);
```

```json
{
  "data": {
    "id": 42
  },
  "attributes": {
    "type": "Int32"
  }
}
```

**`PagedResponse<T>`** - For paginated collections:

```csharp
var users = await _repository.GetPageAsync(page, pageSize, ct);
var totalCount = await _repository.GetTotalCountAsync(ct);

var response = PagedResponse<UserDto>.Create(
    users, 
    totalCount,
    new Dictionary<string, string?>
    {
        { "page", page.ToString() },
        { "pageSize", pageSize.ToString() }
    }
);
```

```json
{
  "data": {
    "items": [
      { "id": 1, "name": "Alice" },
      { "id": 2, "name": "Bob" }
    ]
  },
  "attributes": {
    "totalCount": "150",
    "page": "2",
    "pageSize": "20"
  }
}
```

### Custom Attributes

Add metadata to any response:

```csharp
var attributes = new Dictionary<string, string?>
{
    { "requestId", context.TraceIdentifier },
    { "cached", "true" },
    { "expiresAt", DateTime.UtcNow.AddMinutes(5).ToString("O") }
};

var response = Response<UserDto>.Create(user, attributes);
```

### Predefined Attribute Constants

```csharp
using Hermes.Responses;

var attributes = new Dictionary<string, string?>
{
    { ResponseAttributes.TotalCount, totalCount.ToString() },
    { ResponseAttributes.Type, typeof(User).Name }
};
```

---

## Advanced Scenarios

### Validation Decorator

Create reusable validation logic:

```csharp
public class ValidationCommandHandlerDecorator<TCommand, TResponse> 
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    where TResponse : IResponse
{
    public async Task<Result<TResponse>> Handle(
        TCommand command, 
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new Error(e.ErrorCode, e.ErrorMessage))
                .ToList();
            
            return Result.Ko<TResponse>(errors);
        }
        
        return await _inner.Handle(command, cancellationToken);
    }
}
```

### Caching Decorator

Cache query results automatically:

```csharp
public class CachingQueryHandlerDecorator<TQuery, TResponse> 
    : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : IResponse
{
    public async Task<Result<TResponse>> Handle(
        TQuery query, 
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{typeof(TQuery).Name}:{JsonSerializer.Serialize(query)}";
        
        if (_cache.TryGetValue<Result<TResponse>>(cacheKey, out var cachedResult))
            return cachedResult;
        
        var result = await _inner.Handle(query, cancellationToken);
        
        if (result.IsSuccess)
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        
        return result;
    }
}
```

### Transaction Decorator

Wrap commands in database transactions:

```csharp
public class TransactionCommandHandlerDecorator<TCommand, TResponse> 
    : ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    where TResponse : IResponse
{
    public async Task<Result<TResponse>> Handle(
        TCommand command, 
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var result = await _inner.Handle(command, cancellationToken);
            
            if (result.IsSuccess)
                await transaction.CommitAsync(cancellationToken);
            else
                await transaction.RollbackAsync(cancellationToken);
            
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
```

---

## Best Practices

### Handler Design

1. **Single Responsibility** - One handler per operation
2. **Validate Early** - Return errors before side effects
3. **Async All The Way** - Use async/await with cancellation tokens
4. **Descriptive Errors** - Include error codes and meaningful messages
5. **Collect Errors** - Return all validation errors at once

### Response Design

1. **Consistent Structure** - Always use Response<T> wrappers
2. **Meaningful Attributes** - Add metadata that helps clients
3. **Appropriate Types** - Use `IdResponse<T>` for IDs, `PagedResponse<T>` for lists
4. **Include Type Info** - Especially useful for polymorphic responses

### Error Handling

1. **Error Codes** - Machine-readable error identification
2. **Clear Messages** - Human-readable error descriptions
3. **Metadata** - Additional context (field names, constraints)
4. **Multiple Errors** - Return all validation errors together
5. **No Exceptions** - Use `Result<T>` for business logic failures

### Decorator Order

Order decorators from innermost (closest to handler) to outermost:

1. **Handler** - Core business logic
2. **Logging** - Log execution details
3. **Validation** - Validate before executing
4. **Transaction** - Wrap in database transaction
5. **Caching** - Cache results (queries only)
6. **Authorization** - Check permissions

---

## Why "Hermes"?

In Greek mythology, Hermes was the messenger of the gods, known for delivering messages reliably and consistently across boundaries. Similarly, this library:
- **Delivers** your business logic reliably through handlers
- **Bridges** the gap between clients and services with consistent responses
- **Guides** operations through success and failure paths with the Result pattern

---

## License

This project is licensed under the terms specified in the LICENSE file.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## Support

For issues, questions, or contributions, visit the [GitHub repository](https://github.com/Neelith/Hermes).

## Author

**Neelith**

---

*Structure your API with divine consistency.* ⚡
