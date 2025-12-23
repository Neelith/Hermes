# Hermes Handler and Decorator Examples

This document describes the example handlers and decorators created in the Hermes.WebApi project.

## Overview

The examples demonstrate how to:
1. Create query and command handlers
2. Register handlers using the `AddHandlers` extension method
3. Apply decorators to add cross-cutting concerns (logging, timing, etc.)
4. Decorate all handlers at once with a single decorator
5. Use handlers in minimal API endpoints

## Files Created

### Queries
- **`Queries/GetUserQuery.cs`** - A query to retrieve a user by ID
  - Input: `UserId` (int)
  - Output: `UserResponse`

### Commands
- **`Commands/CreateUserCommand.cs`** - A command to create a new user
  - Input: `Name` (string), `Email` (string)
  - Output: `UserIdResponse`

### Responses
- **`Responses/UserResponse.cs`** - Response containing user information
  - Fields: `Id`, `Name`, `Email`
- **`Responses/UserIdResponse.cs`** - Response containing a user ID
  - Fields: `UserId`

### Handlers
- **`Handlers/GetUserQueryHandler.cs`** - Handles `GetUserQuery`
  - Validates user ID
  - Returns user data or error results
  - Includes logging

- **`Handlers/CreateUserCommandHandler.cs`** - Handles `CreateUserCommand`
  - Validates name and email
  - Creates user and returns ID
  - Includes logging

### Decorators
- **`Decorators/LoggingQueryHandlerDecorator.cs`** - Adds logging to query handlers
  - Logs before and after query execution
  - Measures execution time
  - Logs success/failure with details
  - Catches and logs exceptions

- **`Decorators/LoggingCommandHandlerDecorator.cs`** - Adds logging to command handlers
  - Logs before and after command execution
  - Measures execution time
  - Logs success/failure with details
  - Catches and logs exceptions

## Registration in Program.cs

### Option 1: Decorate Specific Handlers

```csharp
// Register handlers from the WebApi assembly
builder.Services.AddHandlers([Assembly.GetExecutingAssembly()]);

// Register decorators for specific handlers
builder.Services.AddHandlerDecorator<IQueryHandler<GetUserQuery, UserResponse>, 
    LoggingQueryHandlerDecorator<GetUserQuery, UserResponse>>();
    
builder.Services.AddHandlerDecorator<ICommandHandler<CreateUserCommand, UserIdResponse>, 
    LoggingCommandHandlerDecorator<CreateUserCommand, UserIdResponse>>();
```

### Option 2: Decorate All Handlers at Once (Recommended)

**Important:** You need to create a generic decorator that can handle all handler types. Here's an example:

```csharp
// Create a unified logging decorator (you'll need to create this)
public class UnifiedLoggingHandlerDecorator<TRequest, TResponse> : IQueryHandler<TRequest, TResponse>, ICommandHandler<TRequest, TResponse>
    where TRequest : IRequest
    where TResponse : IResponse
{
    private readonly IHandler _inner;
    private readonly ILogger _logger;
    
    // Implementation...
}

// Then register it for all handlers
builder.Services.AddHandlers([Assembly.GetExecutingAssembly()]);

// Decorate ALL query handlers with one call
builder.Services.DecorateAllHandlers(typeof(LoggingQueryHandlerDecorator<,>));

// Or create a unified decorator for both commands and queries
builder.Services.DecorateAllHandlers(typeof(UnifiedLoggingHandlerDecorator<,>));
```

**Note:** The `DecorateAllHandlers` method takes an open generic type (e.g., `typeof(LoggingQueryHandlerDecorator<,>)`) and automatically applies it to all registered handlers of matching types.

## API Endpoints

### GET /api/users/{userId}
Retrieves a user by ID using the `GetUserQueryHandler` with logging decorator.

**Example Request:**
```bash
curl http://localhost:5000/api/users/42
```

**Success Response (200 OK):**
```json
{
  "data": {
    "id": 42,
    "name": "User 42",
    "email": "user42@example.com"
  },
  "metadata": {}
}
```

**Error Response (400 Bad Request):**
```json
{
  "errors": [
    {
      "code": "USER_NOT_FOUND",
      "message": "User with ID 101 not found"
    }
  ]
}
```

### POST /api/users
Creates a new user using the `CreateUserCommandHandler` with logging decorator.

**Example Request:**
```bash
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -d '{"name":"John Doe","email":"john@example.com"}'
```

**Success Response (201 Created):**
```json
{
  "data": {
    "userId": 101
  },
  "metadata": {}
}
```

**Error Response (400 Bad Request):**
```json
{
  "errors": [
    {
      "code": "INVALID_EMAIL",
      "message": "Email must be valid"
    }
  ]
}
```

## Decorator Behavior

The logging decorators add the following behavior to all handlers:

1. **Before Execution:**
   - Log the handler name
   - Log the request details (at Debug level)
   - Start a stopwatch

2. **After Execution:**
   - Stop the stopwatch
   - Log success with execution time
   - OR log failure with error details and execution time

3. **On Exception:**
   - Log the exception with execution time
   - Re-throw the exception

**Example Log Output:**
```
[INFO] [DECORATOR] Before executing query: GetUserQuery
[DEBUG] [DECORATOR] Query details: {"UserId": 42}
[INFO] [DECORATOR] Query GetUserQuery completed successfully in 103ms
```

## How to Add More Decorators

You can create additional decorators for other cross-cutting concerns:

### Validation Decorator
```csharp
public class ValidationQueryHandlerDecorator<TQuery, TResponse> 
    : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : IResponse
{
    private readonly IQueryHandler<TQuery, TResponse> _inner;
    private readonly IValidator<TQuery> _validator;

    public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateAsync(query, ct);
        if (!validationResult.IsValid)
        {
            return Result.Ko<TResponse>(/* validation errors */);
        }
        
        return await _inner.Handle(query, ct);
    }
}
```

### Caching Decorator
```csharp
public class CachingQueryHandlerDecorator<TQuery, TResponse> 
    : IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
    where TResponse : IResponse
{
    private readonly IQueryHandler<TQuery, TResponse> _inner;
    private readonly ICache _cache;

    public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken ct)
    {
        var cacheKey = GenerateCacheKey(query);
        if (_cache.TryGet(cacheKey, out Result<TResponse> cached))
        {
            return cached;
        }
        
        var result = await _inner.Handle(query, ct);
        if (result.IsSuccess)
        {
            _cache.Set(cacheKey, result);
        }
        
        return result;
    }
}
```

### Register Individual Decorators
```csharp
builder.Services.AddHandlerDecorator<IQueryHandler<GetUserQuery, UserResponse>, 
    CachingQueryHandlerDecorator<GetUserQuery, UserResponse>>();
```

### Or Decorate All Handlers at Once
```csharp
// Apply caching to all query handlers
builder.Services.DecorateAllHandlers(typeof(CachingQueryHandlerDecorator<,>));

// Then apply logging on top (decorators stack)
builder.Services.DecorateAllHandlers(typeof(LoggingQueryHandlerDecorator<,>));
```

## Decorator Chain Order

Decorators are applied in the order they are registered. The last registered decorator is the outermost:

### Individual Registration
```csharp
// This creates: Logging -> Validation -> Actual Handler
builder.Services.AddHandlerDecorator<..., ValidationDecorator>();
builder.Services.AddHandlerDecorator<..., LoggingDecorator>();
```

### Bulk Registration
```csharp
// This creates: Logging -> Caching -> Actual Handler
builder.Services.DecorateAllHandlers(typeof(CachingQueryHandlerDecorator<,>));
builder.Services.DecorateAllHandlers(typeof(LoggingQueryHandlerDecorator<,>));
```

## DecorateAllHandlers Method

The `DecorateAllHandlers` extension method provides a convenient way to apply a single decorator type to all registered handlers:

**Signature:**
```csharp
public static IServiceCollection DecorateAllHandlers(
    this IServiceCollection services,
    Type openDecoratorType)
```

**Parameters:**
- `openDecoratorType`: An open generic type (e.g., `typeof(LoggingQueryHandlerDecorator<,>)`) that will be applied to all matching handlers

**How it works:**
1. Scans for all registered handlers (Query and Command handlers)
2. For each handler, creates a closed generic version of the decorator
3. Wraps the handler with the decorator using Scrutor's `Decorate` method

**Supported Handler Types:**
- `IQueryHandler<TQuery, TResponse>`
- `ICommandHandler<TCommand>` (without response)
- `ICommandHandler<TCommand, TResponse>` (with response)

**Example:**
```csharp
// Instead of decorating each handler individually
builder.Services.AddHandlerDecorator<IQueryHandler<GetUserQuery, UserResponse>, LoggingQueryHandlerDecorator<GetUserQuery, UserResponse>>();
builder.Services.AddHandlerDecorator<IQueryHandler<GetOrderQuery, OrderResponse>, LoggingQueryHandlerDecorator<GetOrderQuery, OrderResponse>>();
// ... many more lines

// Simply use:
builder.Services.DecorateAllHandlers(typeof(LoggingQueryHandlerDecorator<,>));
```

This is especially useful when you have many handlers and want to apply the same cross-cutting concern to all of them.

## Notes

- All decorators must implement the same interface as the handler they decorate
- Decorators receive the inner handler through constructor injection
- The `AddHandlerDecorator` extension uses Scrutor's `TryDecorate` method internally
- The `DecorateAllHandlers` extension uses Scrutor's `Decorate` method with a factory function
- Response types must implement `IResponse` interface
- Commands must implement `ICommand<TResponse>` interface
- Queries must implement `IQuery<TResponse>` interface
