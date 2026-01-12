# Development Guide - Sky.Cms.Api.Shared

## Overview

This guide explains how to extend the Sky.Cms.Api.Shared API with new endpoints and features.

## Adding a New Endpoint

Follow these steps to add a new API endpoint:

### Step 1: Create Models

Create your request and response DTOs in the `Models/` folder:

```csharp
// Models/MyFeatureRequest.cs
namespace Sky.Cms.Api.Shared.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request model for my feature.
/// </summary>
public class MyFeatureRequest
{
    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Data { get; set; } = string.Empty;
}

// Models/MyFeatureResponse.cs
namespace Sky.Cms.Api.Shared.Models;

/// <summary>
/// Response model for my feature.
/// </summary>
public class MyFeatureResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the response message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
```

### Step 2: Create Feature Folder and CQRS Classes

Create a folder in `Features/` for your feature:

```
Features/MyFeature/
├── Submit/
│   ├── MyFeatureCommand.cs
│   └── MyFeatureHandler.cs
└── YourOtherFile.cs
```

**MyFeatureCommand.cs**:
```csharp
// Features/MyFeature/Submit/MyFeatureCommand.cs
namespace Sky.Cms.Api.Shared.Features.MyFeature.Submit;

using Sky.Cms.Api.Shared.Models;
using Sky.Cms.Api.Shared.Features.Shared;

/// <summary>
/// Command to process my feature.
/// </summary>
public class MyFeatureCommand : ICommand<MyFeatureResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MyFeatureCommand"/> class.
    /// </summary>
    /// <param name="request">The request data.</param>
    public MyFeatureCommand(MyFeatureRequest request)
    {
        this.Request = request;
    }

    /// <summary>
    /// Gets the request data.
    /// </summary>
    public MyFeatureRequest Request { get; }
}
```

**MyFeatureHandler.cs**:
```csharp
// Features/MyFeature/Submit/MyFeatureHandler.cs
namespace Sky.Cms.Api.Shared.Features.MyFeature.Submit;

using Microsoft.Extensions.Logging;
using Sky.Cms.Api.Shared.Models;
using Sky.Cms.Api.Shared.Features.Shared;

/// <summary>
/// Handler for my feature command.
/// </summary>
public class MyFeatureHandler : ICommandHandler<MyFeatureCommand, MyFeatureResponse>
{
    private readonly ILogger<MyFeatureHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyFeatureHandler"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public MyFeatureHandler(ILogger<MyFeatureHandler> logger)
    {
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<MyFeatureResponse> HandleAsync(
        MyFeatureCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            this.logger.LogInformation("Processing my feature");

            // Implement your business logic here
            var result = ProcessMyFeature(command.Request);

            return new MyFeatureResponse
            {
                Success = true,
                Message = "Operation completed successfully"
            };
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error processing my feature");
            return new MyFeatureResponse
            {
                Success = false,
                Message = "An error occurred"
            };
        }
    }

    private bool ProcessMyFeature(MyFeatureRequest request)
    {
        // Your implementation here
        return true;
    }
}
```

### Step 3: Create Service (Optional)

If your feature has complex business logic, create a service:

```csharp
// Services/IMyFeatureService.cs
namespace Sky.Cms.Api.Shared.Services;

using Sky.Cms.Api.Shared.Models;

/// <summary>
/// Interface for my feature service.
/// </summary>
public interface IMyFeatureService
{
    /// <summary>
    /// Process my feature.
    /// </summary>
    /// <param name="request">Request data.</param>
    /// <returns>Task representing the async operation.</returns>
    Task<MyFeatureResponse> ProcessAsync(MyFeatureRequest request);
}

// Services/MyFeatureService.cs
namespace Sky.Cms.Api.Shared.Services;

using Microsoft.Extensions.Logging;
using Sky.Cms.Api.Shared.Models;

/// <summary>
/// Service for my feature.
/// </summary>
public class MyFeatureService : IMyFeatureService
{
    private readonly ILogger<MyFeatureService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyFeatureService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public MyFeatureService(ILogger<MyFeatureService> logger)
    {
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<MyFeatureResponse> ProcessAsync(MyFeatureRequest request)
    {
        this.logger.LogInformation("Processing my feature");
        // Your implementation here
        return new MyFeatureResponse
        {
            Success = true,
            Message = "Processed successfully"
        };
    }
}
```

### Step 4: Register in DI Container

Update `ContactApiServiceExtensions.cs`:

```csharp
public static IServiceCollection AddContactApi(this IServiceCollection services, IConfiguration configuration)
{
    // ... existing code ...

    // Register new service
    services.AddScoped<IMyFeatureService, MyFeatureService>();

    // Register handler
    services.AddScoped<ICommandHandler<MyFeatureCommand, MyFeatureResponse>, MyFeatureHandler>();

    return services;
}
```

### Step 5: Add Controller Endpoint

Add a method to `ContactApiController.cs`:

```csharp
/// <summary>
/// Processes my feature.
/// </summary>
/// <param name="request">My feature request.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>My feature response.</returns>
[HttpPost("my-feature")]
[ProducesResponseType(typeof(MyFeatureResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(MyFeatureResponse), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> ProcessMyFeature(
    [FromBody] MyFeatureRequest request,
    CancellationToken cancellationToken = default)
{
    try
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new MyFeatureResponse
            {
                Success = false,
                Message = "Validation failed"
            });
        }

        var command = new MyFeatureCommand(request);
        var result = await mediator.SendAsync(command, cancellationToken);

        return Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing my feature");
        return StatusCode(500, new MyFeatureResponse
        {
            Success = false,
            Message = "An error occurred"
        });
    }
}
```

## CQRS Pattern Usage

### Commands

Use commands for operations that modify state or have side effects:

```csharp
public interface ICommand<TResult>
{
    // Marker interface
}

public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
```

Example: Submitting a contact form (has side effect of sending email)

### Queries

Use queries for read-only operations:

```csharp
public interface IQuery<TResult>
{
    // Marker interface
}

public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
```

Example: Validating a CAPTCHA token (no side effects, just returns result)

## Configuration-Driven Features

To make your feature configurable:

1. **Add properties to a config class**:
   ```csharp
   public class MyFeatureConfig
   {
       public string ApiKey { get; set; } = string.Empty;
       public int MaxRetries { get; set; } = 3;
   }
   ```

2. **Register in DI**:
   ```csharp
   services.Configure<MyFeatureConfig>(configuration.GetSection("MyFeature"));
   ```

3. **Use in your service**:
   ```csharp
   public MyFeatureService(IOptions<MyFeatureConfig> config)
   {
       this.config = config.Value;
   }
   ```

4. **Configure in appsettings.json**:
   ```json
   {
     "MyFeature": {
       "ApiKey": "value",
       "MaxRetries": 5
     }
   }
   ```

## Testing Your Feature

### Unit Testing a Handler

```csharp
[TestClass]
public class MyFeatureHandlerTests
{
    private MyFeatureHandler handler;
    private Mock<ILogger<MyFeatureHandler>> loggerMock;

    [TestInitialize]
    public void Setup()
    {
        loggerMock = new Mock<ILogger<MyFeatureHandler>>();
        handler = new MyFeatureHandler(loggerMock.Object);
    }

    [TestMethod]
    public async Task HandleAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var command = new MyFeatureCommand(new MyFeatureRequest { Data = "test" });

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Operation completed successfully", result.Message);
    }
}
```

### Integration Testing

```csharp
[TestClass]
public class MyFeatureIntegrationTests
{
    private readonly HttpClient httpClient;

    public MyFeatureIntegrationTests()
    {
        // Use a test host or in-memory test server
        var host = new TestServerBuilder()
            .AddContactApi()
            .Build();
        
        httpClient = host.CreateClient();
    }

    [TestMethod]
    public async Task PostMyFeature_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var request = new MyFeatureRequest { Data = "test" };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await httpClient.PostAsync("/_api/my-feature", content);
        var result = await response.Content.ReadAsAsync<MyFeatureResponse>();

        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode);
        Assert.IsTrue(result.Success);
    }
}
```

## Best Practices

1. **Always validate input**: Use model attributes (`[Required]`, `[StringLength]`, etc.)
2. **Log important operations**: Use `ILogger<T>` for debugging
3. **Handle exceptions gracefully**: Return meaningful error messages
4. **Use async/await**: Leverage async APIs for better scalability
5. **Follow naming conventions**: Commands end with `Command`, queries with `Query`
6. **Document with XML comments**: Help future developers understand your code
7. **Create interfaces**: Depend on abstractions for testability
8. **Keep handlers focused**: Each handler should do one thing
9. **Use dependency injection**: Never instantiate dependencies directly
10. **Add rate limiting if needed**: For endpoints that could be abused

## Common Patterns

### Feature with Database Operations

```csharp
public class SaveMyFeatureHandler : ICommandHandler<SaveMyFeatureCommand, MyFeatureResponse>
{
    private readonly IMyFeatureRepository repository;
    private readonly ILogger<SaveMyFeatureHandler> logger;

    public SaveMyFeatureHandler(IMyFeatureRepository repository, ILogger<SaveMyFeatureHandler> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    public async Task<MyFeatureResponse> HandleAsync(SaveMyFeatureCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await repository.SaveAsync(command.Request, cancellationToken);
            return new MyFeatureResponse { Success = true, Message = "Saved" };
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to save");
            return new MyFeatureResponse { Success = false, Message = "Failed to save" };
        }
    }
}
```

### Feature with External API Call

```csharp
public class CallExternalApiHandler : ICommandHandler<CallExternalApiCommand, MyFeatureResponse>
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<CallExternalApiHandler> logger;

    public CallExternalApiHandler(IHttpClientFactory httpClientFactory, ILogger<CallExternalApiHandler> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    public async Task<MyFeatureResponse> HandleAsync(CallExternalApiCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://api.example.com/data", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                this.logger.LogError("External API returned {StatusCode}", response.StatusCode);
                return new MyFeatureResponse { Success = false };
            }

            return new MyFeatureResponse { Success = true };
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error calling external API");
            return new MyFeatureResponse { Success = false };
        }
    }
}
```

## Troubleshooting Development

### Handler not being called
- Verify it's registered in `AddContactApi()` in `ContactApiServiceExtensions.cs`
- Check that the handler type matches the command/query type

### Configuration not loading
- Verify the section name in `appsettings.json` matches the configuration property
- Check that configuration is registered with `services.Configure<T>`

### Dependency injection errors
- Verify all constructor parameters are registered in DI
- Check for circular dependencies

### Test failures
- Ensure mock objects are set up correctly
- Verify async/await usage is correct
- Check that test data is valid

## Next Steps

Once you've added a new endpoint:

1. **Write unit tests** for your handler
2. **Write integration tests** for your controller endpoint
3. **Update documentation** with endpoint details
4. **Test with client applications** (Editor and Publisher)
5. **Consider security implications** (validation, rate limiting, authentication)
6. **Add configuration options** if the feature needs customization
