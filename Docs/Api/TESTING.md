# Testing Guide - Sky.Cms.Api.Shared

## Overview

This guide covers testing strategies for the Sky.Cms.Api.Shared API, including unit tests, integration tests, and manual testing approaches.

## Test Structure

The test project (`AspNetCore.Identity.FlexDb.Tests` or similar) should follow this structure:

```
Tests/
├── Features/
│   └── ContactForm/
│       ├── Submit/
│       │   └── SubmitContactFormHandlerTests.cs
│       └── ValidateCaptcha/
│           └── ValidateCaptchaHandlerTests.cs
├── Services/
│   ├── ContactServiceTests.cs
│   └── CaptchaValidatorTests.cs
├── Controllers/
│   └── ContactApiControllerTests.cs
└── TestHelpers/
    ├── TestDataBuilder.cs
    └── MockFactories.cs
```

## Unit Testing

### Testing a Handler

**Example: SubmitContactFormHandler**

```csharp
namespace Sky.Cms.Api.Shared.Tests.Features.ContactForm.Submit;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sky.Cms.Api.Shared.Features.ContactForm.Submit;
using Sky.Cms.Api.Shared.Models;
using Microsoft.Extensions.Logging;
using Cosmos.EmailServices;
using Microsoft.Extensions.Options;

[TestClass]
public class SubmitContactFormHandlerTests
{
    private Mock<ICosmosEmailSender> emailSenderMock;
    private Mock<ILogger<SubmitContactFormHandler>> loggerMock;
    private SubmitContactFormHandler handler;
    private ContactApiConfig config;

    [TestInitialize]
    public void Setup()
    {
        emailSenderMock = new Mock<ICosmosEmailSender>();
        loggerMock = new Mock<ILogger<SubmitContactFormHandler>>();
        
        config = new ContactApiConfig
        {
            AdminEmail = "admin@test.com",
            MaxMessageLength = 5000,
            RequireCaptcha = false
        };

        var optionsMock = Options.Create(config);
        handler = new SubmitContactFormHandler(emailSenderMock.Object, loggerMock.Object, optionsMock);
    }

    [TestMethod]
    public async Task HandleAsync_WithValidRequest_SendsEmail()
    {
        // Arrange
        var request = new ContactFormRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Message = "This is a test message that is longer than 10 characters"
        };

        var command = new SubmitContactFormCommand(request);

        // Mock successful email send
        var mockResult = new Mock<IHttpResponseMessage>();
        mockResult.Setup(r => r.IsSuccessStatusCode).Returns(true);
        emailSenderMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(mockResult.Object);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Thank you for your message. We'll get back to you soon!", result.Message);
        
        // Verify email was sent
        emailSenderMock.Verify(
            e => e.SendEmailAsync(
                config.AdminEmail,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                request.Email),
            Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_WithEmailSendFailure_ReturnsFailureResponse()
    {
        // Arrange
        var request = new ContactFormRequest
        {
            Name = "Jane Smith",
            Email = "jane@example.com",
            Message = "This is another test message for failure scenario"
        };

        var command = new SubmitContactFormCommand(request);

        // Mock failed email send
        var mockResult = new Mock<IHttpResponseMessage>();
        mockResult.Setup(r => r.IsSuccessStatusCode).Returns(false);
        mockResult.Setup(r => r.StatusCode).Returns(System.Net.HttpStatusCode.InternalServerError);
        emailSenderMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(mockResult.Object);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("Email delivery failed", result.Error);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WithNullEmailSender_ThrowsException()
    {
        // Should throw ArgumentNullException
        var handler = new SubmitContactFormHandler(null, loggerMock.Object, Options.Create(config));
    }
}
```

### Testing a Service

**Example: ContactService CAPTCHA Validation**

```csharp
namespace Sky.Cms.Api.Shared.Tests.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sky.Cms.Api.Shared.Services;
using Sky.Cms.Api.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;

[TestClass]
public class ContactServiceCaptchaTests
{
    private Mock<IHttpClientFactory> httpClientFactoryMock;
    private Mock<ICosmosEmailSender> emailSenderMock;
    private Mock<ILogger<ContactService>> loggerMock;
    private ContactService service;

    [TestInitialize]
    public void Setup()
    {
        httpClientFactoryMock = new Mock<IHttpClientFactory>();
        emailSenderMock = new Mock<ICosmosEmailSender>();
        loggerMock = new Mock<ILogger<ContactService>>();
    }

    private void CreateService(ContactApiConfig config)
    {
        var optionsMock = Options.Create(config);
        service = new ContactService(httpClientFactoryMock.Object, emailSenderMock.Object, loggerMock.Object, optionsMock);
    }

    [TestMethod]
    public async Task ValidateCaptchaAsync_WithDisabledCaptcha_ReturnsTrue()
    {
        // Arrange
        var config = new ContactApiConfig { RequireCaptcha = false };
        CreateService(config);

        // Act
        var result = await service.ValidateCaptchaAsync("any-token", "192.168.1.1");

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ValidateCaptchaAsync_WithTurnstileSuccess_ReturnsTrue()
    {
        // Arrange
        var config = new ContactApiConfig
        {
            RequireCaptcha = true,
            CaptchaProvider = "turnstile",
            CaptchaSecretKey = "test-secret"
        };
        CreateService(config);

        var mockHttpClient = new Mock<HttpClient>();
        var mockResponse = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{ ""success"": true }")
        };

        httpClientFactoryMock.Setup(f => f.CreateClient())
            .Returns(mockHttpClient.Object);

        // Act & Assert - Implementation details would follow

    }

    [TestMethod]
    public async Task ValidateCaptchaAsync_WithTurnstileFailure_ReturnsFalse()
    {
        // Arrange
        var config = new ContactApiConfig
        {
            RequireCaptcha = true,
            CaptchaProvider = "turnstile",
            CaptchaSecretKey = "test-secret"
        };
        CreateService(config);

        // Act & Assert - Implementation details would follow
    }
}
```

### Model Validation Tests

```csharp
namespace Sky.Cms.Api.Shared.Tests.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;
using Sky.Cms.Api.Shared.Models;

[TestClass]
public class ContactFormRequestValidationTests
{
    private ContactFormRequest request;

    [TestInitialize]
    public void Setup()
    {
        request = new ContactFormRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Message = "This is a valid test message"
        };
    }

    [TestMethod]
    public void ValidRequest_PassesValidation()
    {
        // Arrange & Act
        var validationContext = new ValidationContext(request);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, validationContext, results, true);

        // Assert
        Assert.IsTrue(isValid);
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void EmptyName_FailsValidation()
    {
        // Arrange
        request.Name = string.Empty;

        // Act
        var validationContext = new ValidationContext(request);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, validationContext, results, true);

        // Assert
        Assert.IsFalse(isValid);
        Assert.IsTrue(results.Any(r => r.MemberNames.Contains(nameof(ContactFormRequest.Name))));
    }

    [TestMethod]
    public void NameTooShort_FailsValidation()
    {
        // Arrange
        request.Name = "J";

        // Act
        var validationContext = new ValidationContext(request);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, validationContext, results, true);

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void InvalidEmail_FailsValidation()
    {
        // Arrange
        request.Email = "not-an-email";

        // Act
        var validationContext = new ValidationContext(request);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, validationContext, results, true);

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void MessageTooShort_FailsValidation()
    {
        // Arrange
        request.Message = "short";

        // Act
        var validationContext = new ValidationContext(request);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, validationContext, results, true);

        // Assert
        Assert.IsFalse(isValid);
    }

    [TestMethod]
    public void MessageTooLong_FailsValidation()
    {
        // Arrange
        request.Message = new string('a', 5001);

        // Act
        var validationContext = new ValidationContext(request);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, validationContext, results, true);

        // Assert
        Assert.IsFalse(isValid);
    }
}
```

## Integration Testing

### Testing Controller Endpoints

```csharp
namespace Sky.Cms.Api.Shared.Tests.Controllers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http.Json;
using Sky.Cms.Api.Shared.Extensions;
using Sky.Cms.Api.Shared.Models;

[TestClass]
public class ContactApiControllerIntegrationTests
{
    private TestServer testServer;
    private HttpClient client;

    [TestInitialize]
    public void Setup()
    {
        var builder = WebApplication.CreateBuilder();
        
        builder.Services.AddAntiforgery();
        builder.Services.AddLogging();
        builder.Services.AddControllers();
        builder.Services.AddRateLimiter(options =>
        {
            ContactApiServiceExtensions.ConfigureContactApiRateLimiting(options);
        });
        
        builder.Configuration["ContactApi:AdminEmail"] = "test@example.com";
        builder.Services.AddContactApi(builder.Configuration);

        var app = builder.Build();
        
        app.UseRouting();
        app.UseRateLimiter();
        app.MapControllers();

        testServer = new TestServer(app);
        client = testServer.CreateClient();
    }

    [TestMethod]
    public async Task GetContactScript_ReturnsJavaScript()
    {
        // Act
        var response = await client.GetAsync("/_api/contact/skycms-contact.js");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/javascript", response.Content.Headers.ContentType?.MediaType);
    }

    [TestMethod]
    public async Task PostSubmit_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new ContactFormRequest
        {
            Name = "Integration Test",
            Email = "test@example.com",
            Message = "This is an integration test message"
        };

        // Act
        var response = await client.PostAsJsonAsync("/_api/contact/submit", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadAsAsync<ContactFormResponse>();
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public async Task PostSubmit_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new ContactFormRequest
        {
            Name = "Test",
            Email = "invalid-email",
            Message = "This is a test message with invalid email"
        };

        // Act
        var response = await client.PostAsJsonAsync("/_api/contact/submit", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestCleanup]
    public void Cleanup()
    {
        client?.Dispose();
        testServer?.Dispose();
    }
}
```

## Performance Testing

### Load Testing Example

```csharp
namespace Sky.Cms.Api.Shared.Tests.Performance;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Net.Http.Json;
using Sky.Cms.Api.Shared.Models;

[TestClass]
public class ContactApiPerformanceTests
{
    private HttpClient client;

    [TestInitialize]
    public void Setup()
    {
        // Initialize test server
        client = new HttpClient { BaseAddress = new Uri("https://localhost:5001") };
    }

    [TestMethod]
    [Timeout(5000)] // 5 second timeout
    public async Task PostSubmit_CompletesWithinAcceptableTime()
    {
        // Arrange
        var request = new ContactFormRequest
        {
            Name = "Performance Test",
            Email = "perf@test.com",
            Message = "This is a performance test message"
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await client.PostAsJsonAsync("/_api/contact/submit", request);

        stopwatch.Stop();

        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode);
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000, 
            $"Request took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
    }
}
```

## Manual Testing

### cURL Commands

```bash
# Get contact script
curl -X GET "https://localhost:5001/_api/contact/skycms-contact.js" \
  -H "Accept: application/javascript"

# Submit contact form
curl -X POST "https://localhost:5001/_api/contact/submit" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john@example.com",
    "message": "This is a test message that is at least 10 characters long"
  }'
```

### Postman Collection

Create a Postman collection with these requests:

1. **Get Contact Script**
   - Method: GET
   - URL: `{{base_url}}/_api/contact/skycms-contact.js`
   - Headers: Accept: application/javascript

2. **Submit Valid Form**
   - Method: POST
   - URL: `{{base_url}}/_api/contact/submit`
   - Body (JSON):
     ```json
     {
       "name": "Test User",
       "email": "test@example.com",
       "message": "This is a test message"
     }
     ```

3. **Submit with CAPTCHA**
   - Method: POST
   - URL: `{{base_url}}/_api/contact/submit`
   - Body (JSON):
     ```json
     {
       "name": "Test User",
       "email": "test@example.com",
       "message": "This is a test message",
       "captchaToken": "your-captcha-token"
     }
     ```

## Test Coverage Goals

Aim for the following coverage targets:

| Component | Target Coverage |
|-----------|-----------------|
| Handlers | 90%+ |
| Services | 90%+ |
| Models/DTOs | 100% (validation) |
| Controllers | 85%+ |
| Overall | 85%+ |

## Continuous Integration

### GitHub Actions Example

```yaml
name: Test

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
    
    - name: Upload coverage
      uses: codecov/codecov-action@v2
      with:
        files: ./coverage.opencover.xml
```

## Best Practices

1. **Isolate dependencies**: Use mocks for external services
2. **Test behavior, not implementation**: Focus on what, not how
3. **Use descriptive test names**: Should read like documentation
4. **Keep tests independent**: No test should depend on another
5. **Use Arrange-Act-Assert pattern**: Clear structure for readability
6. **Test edge cases**: Empty strings, nulls, boundary values
7. **Test error paths**: Not just the happy path
8. **Use test data builders**: Reduce boilerplate in test setup
9. **Mock external APIs**: Don't call real APIs in tests
10. **Verify logging**: Ensure important events are logged

## Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "ClassName=MyTest"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# Run in verbose mode
dotnet test --verbosity normal

# Run with watch mode (auto-run on file changes)
dotnet watch test
```
