# Sky.Cms.Api.Shared Architecture

## Overview

Sky.Cms.Api.Shared uses a custom CQRS (Command Query Responsibility Segregation) pattern combined with dependency injection to create a clean, testable, and maintainable API architecture.

## Design Patterns

### CQRS Pattern

The API separates read and write operations:

- **Commands**: Operations that modify state (e.g., submitting a contact form)
- **Queries**: Operations that retrieve data (e.g., validating a CAPTCHA)
- **Handlers**: Classes that execute commands or queries
- **Mediator**: Acts as a dispatcher between controllers and handlers

This pattern provides:
- Clear separation of concerns
- Easier unit testing
- Better maintainability as the API grows
- Logical organization of business logic

### Service Layer Pattern

Services encapsulate business logic and external integrations:
- `IContactService` - Handles contact form processing and CAPTCHA validation
- `ICaptchaValidator` - Abstract interface for CAPTCHA validation strategies

### Dependency Injection

All dependencies are registered in `ContactApiServiceExtensions.cs`, making the code:
- Loosely coupled
- Testable with mock implementations
- Easy to swap implementations (e.g., different CAPTCHA providers)

## Directory Structure

```
Sky.Cms.Api.Shared/
├── Controllers/
│   └── ContactApiController.cs          # HTTP API endpoints
├── Models/
│   ├── ContactApiConfig.cs              # Configuration model
│   ├── ContactFormRequest.cs            # Request DTO
│   ├── ContactFormResponse.cs           # Response DTO
│   └── CaptchaValidationRequest.cs      # Captcha validation DTO
├── Services/
│   ├── ContactService.cs                # Business logic for contact form
│   ├── IContactService.cs               # Interface
│   ├── ICaptchaValidator.cs             # Interface for CAPTCHA
│   └── Captcha/
│       └── NoOpCaptchaValidator.cs      # Default no-op implementation
├── Features/
│   ├── ContactForm/
│   │   ├── Submit/
│   │   │   ├── SubmitContactFormCommand.cs    # Command
│   │   │   └── SubmitContactFormHandler.cs    # Command handler
│   │   └── ValidateCaptcha/
│   │       ├── ValidateCaptchaQuery.cs        # Query
│   │       └── ValidateCaptchaHandler.cs      # Query handler
│   └── Shared/
│       ├── ICommand.cs                  # Base command interface
│       ├── ICommandHandler.cs           # Base handler interface
│       ├── IQuery.cs                    # Base query interface
│       ├── IQueryHandler.cs             # Base handler interface
│       ├── IMediator.cs                 # Mediator interface
│       └── CommandResult.cs             # Generic result wrapper
└── Extensions/
    └── ContactApiServiceExtensions.cs   # DI setup and configuration

```

## Request Flow

### Contact Form Submission

```
1. HTTP POST /_api/contact/submit
   ↓
2. ContactApiController.Submit()
   ↓
3. Anti-forgery token validation
   ↓
4. Model state validation
   ↓
5. Dispatch SubmitContactFormCommand via IMediator
   ↓
6. SubmitContactFormHandler.HandleAsync()
   ↓
7. ContactService.SubmitContactFormAsync()
   ├─→ Build email content
   ├─→ Send via ICosmosEmailSender
   └─→ Return result
   ↓
8. HTTP Response
```

### CAPTCHA Validation

```
1. Client requests CAPTCHA token validation
   ↓
2. Dispatch ValidateCaptchaQuery via IMediator
   ↓
3. ValidateCaptchaHandler.HandleAsync()
   ↓
4. ContactService.ValidateCaptchaAsync()
   ├─→ Check configuration
   ├─→ Route to appropriate provider (Turnstile or reCAPTCHA)
   └─→ Return validation result
```

## Key Components

### ContactApiController
- Exposes HTTP endpoints
- Handles anti-forgery token management
- Coordinates CQRS dispatch
- Returns standardized responses

### ContactService
- Implements core business logic
- Integrates with email service (ICosmosEmailSender)
- Supports multiple CAPTCHA providers
- Includes comprehensive error handling

### Feature Handlers
- **SubmitContactFormHandler**: Processes contact form submissions
- **ValidateCaptchaHandler**: Validates CAPTCHA tokens

## Extensibility

Adding new endpoints is straightforward:

1. Create a new feature folder: `Features/YourFeature/`
2. Define command/query: `YourCommand.cs` or `YourQuery.cs`
3. Implement handler: `YourHandler.cs`
4. Create service interface and implementation if needed
5. Register in `ContactApiServiceExtensions.cs`
6. Add controller method in `ContactApiController.cs` (or new controller)

## Configuration-Driven Behavior

The API uses configuration (via `ContactApiConfig`) to control behavior:
- **CAPTCHA Provider**: Auto-detected from configuration
- **Email Target**: Configurable admin email
- **Message Length**: Configurable via `MaxMessageLength`
- **Rate Limiting**: Per-IP rate limit configuration

This makes the API flexible without code changes.

## Security Features

1. **Anti-forgery Protection**: Token validation on POST requests
2. **Rate Limiting**: 5 requests per minute per IP address
3. **CAPTCHA Validation**: Optional but recommended in production
4. **Input Validation**: Strict model validation on all DTOs
5. **Email Validation**: RFC 5322 compliant email validation
6. **Error Masking**: Generic error messages to prevent information disclosure

## Error Handling

Errors are handled at multiple levels:
1. **Controller level**: Invalid model state, framework errors
2. **Handler level**: Validation and processing errors
3. **Service level**: External API failures, email sending failures

All errors are logged with contextual information (IP address, email, etc.) for debugging.

## Logging

Comprehensive logging is implemented using standard `ILogger<T>`:
- `Information`: Successful operations
- `Warning`: Validation failures, external API issues
- `Error`: Service failures, exceptions

Access logs include:
- Submitted email address
- Remote IP address
- Timestamp
- Operation status
- Error details (when applicable)
