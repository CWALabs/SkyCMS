---
title: Middleware Pipeline Documentation
description: ASP.NET Core middleware pipeline architecture explaining component execution order and request/response processing
keywords: middleware, pipeline, ASP.NET-Core, architecture, request-processing
audience: [developers, architects]
---

# Middleware Pipeline Documentation

The ASP.NET Core middleware pipeline in SkyCMS is a sequence of components that process HTTP requests and responses. This guide explains each middleware component, their execution order, and how they work together.

## Table of Contents
- [Middleware Pipeline Overview](#middleware-pipeline-overview)
- [Middleware Execution Order](#middleware-execution-order)
- [Core Middleware Components](#core-middleware-components)
- [Middleware Details](#middleware-details)
- [Request and Response Flow](#request-and-response-flow)
- [Custom Middleware](#custom-middleware)
- [Debugging Middleware](#debugging-middleware)
- [Best Practices](#best-practices)

## Middleware Pipeline Overview

**Middleware** is software that's assembled into an app pipeline to handle requests and responses. Each middleware component:

1. **Decides** whether to pass the request to the next middleware
2. **Can modify** the request before passing it forward
3. **Can modify** the response after it comes back from the next middleware
4. **Can short-circuit** the pipeline and return immediately

### Pipeline Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    HTTP Request Arrives                      │
└─────────────────────────────────────────────────────────────┘
                            ↓
                  [Middleware Chain]
                            ↓
        Request flows through middlewares →
        ← Response flows back through middlewares
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    HTTP Response Sent                        │
└─────────────────────────────────────────────────────────────┘
```

## Middleware Execution Order

**Order is critical!** Middleware executes in the order registered in `Program.cs`.

### SkyCMS Middleware Sequence

```
[1] ForwardedHeaders Middleware
    ↓
[2] DomainMiddleware (Multi-tenant only)
    ↓
[3] CosmosCmsDataProtection Middleware
    ↓
[4] Setup Check Inline Middleware
    ↓
[5] SetupRedirectMiddleware
    ↓
[6] DeveloperExceptionPage / ExceptionHandler
    ↓
[7] StaticFiles Middleware
    ↓
[8] Routing Middleware
    ↓
[9] Cors Middleware
    ↓
[10] ResponseCaching Middleware
    ↓
[11] Authentication Middleware
    ↓
[12] Authorization Middleware
    ↓
[13] RateLimiter Middleware
    ↓
[14] Endpoint (Controller Action)
```

**Important:** Registration order in code = execution order in request.

## Core Middleware Components

| Middleware | Purpose | Location |
|------------|---------|----------|
| **ForwardedHeaders** | Detect proxies, set X-Forwarded-* headers | ASP.NET Core built-in |
| **DomainMiddleware** | Multi-tenant domain extraction and validation | Cosmos.ConnectionStrings |
| **DataProtection** | Initialize data protection service | AspNetCore.Identity.FlexDb |
| **Setup Check** | Verify setup wizard isn't bypassed | Program.cs (inline) |
| **SetupRedirectMiddleware** | Redirect to setup if not configured | Sky.Editor.Middleware |
| **ExceptionHandler** | Catch unhandled exceptions | ASP.NET Core built-in |
| **StaticFiles** | Serve CSS, JS, images, etc. | ASP.NET Core built-in |
| **Routing** | Match request to controller/action | ASP.NET Core built-in |
| **CORS** | Handle cross-origin requests | ASP.NET Core built-in |
| **ResponseCaching** | Cache responses per configuration | ASP.NET Core built-in |
| **Authentication** | Identify the user | ASP.NET Core built-in |
| **Authorization** | Check if user has access | ASP.NET Core built-in |
| **RateLimiter** | Throttle excessive requests | ASP.NET Core built-in |

## Middleware Details

### 1. ForwardedHeaders Middleware

**Purpose:** Detect proxy scenarios and set proper request properties

```csharp
app.UseForwardedHeaders();
```

**What it does:**
- Reads `X-Forwarded-For`, `X-Forwarded-Proto` headers from proxies
- Updates `HttpContext.Connection.RemoteIpAddress`
- Updates `HttpContext.Request.Scheme` (http vs https)

**Example:**
```
Client Request: https://example.com/page
  ↓ (HTTPS)
Load Balancer (changes scheme to HTTP internally)
  ↓ (HTTP)
SkyCMS (sees HTTP)

Without ForwardedHeaders:
  - Request.Scheme = "http" ❌
  - Cookies sent as insecure
  - OAuth redirects use http

With ForwardedHeaders:
  - Request.Scheme = "https" ✓ (from X-Forwarded-Proto header)
  - Cookies sent as secure
  - OAuth redirects use https
```

**Configuration:**
```csharp
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | 
                               ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();  // Trust all proxies in dev/test
    options.KnownProxies.Clear();
});
```

### 2. DomainMiddleware (Multi-Tenant Only)

**Purpose:** Extract and validate tenant domain for multi-tenant setups

```csharp
if (isMultiTenantEditor)
    app.UseMiddleware<DomainMiddleware>();
```

**What it does:**
1. Extracts domain from `Host` header
2. Queries `IDynamicConfigurationProvider` to validate domain
3. Returns 404 if domain not configured
4. Sets `context.Items["Domain"]` for downstream use

**Execution:**
```csharp
public async Task InvokeAsync(HttpContext context)
{
    var domain = context.Request.Host.Host.ToLowerInvariant();
    
    var configProvider = context.RequestServices
        .GetService<IDynamicConfigurationProvider>();
    
    // Validate domain has connection configuration
    var connectionString = await configProvider
        .GetDatabaseConnectionStringAsync(domain);
    
    if (string.IsNullOrEmpty(connectionString))
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Not Found");
        return;
    }
    
    context.Items["Domain"] = domain;
    await next(context);
}
```

**When it runs:**
- Multi-tenant mode only
- Before DbContext usage (domain needed for connection)
- Early in pipeline for quick rejection

### 3. CosmosCmsDataProtection Middleware

**Purpose:** Initialize data protection for encrypted fields

```csharp
app.UseCosmosCmsDataProtection();
```

**What it does:**
- Sets up encryption/decryption for sensitive data
- Initializes key storage
- Validates data protection provider

### 4. Setup Check Inline Middleware

**Purpose:** Prevent accessing sensitive endpoints if setup hasn't completed

```csharp
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/___setup"))
    {
        var config = context.RequestServices
            .GetRequiredService<IConfiguration>();
        var allowSetup = config.GetValue<bool?>("CosmosAllowSetup") ?? false;

        if (!allowSetup)
        {
            context.Response.Redirect("/");
            return;
        }
    }

    await next(context);
});
```

**What it does:**
- Checks if setup wizard access is enabled
- Redirects to home if setup is disabled but someone tries to access it
- Prevents setup bypass

### 5. SetupRedirectMiddleware

**Purpose:** Automatically redirect to setup wizard if not configured

```csharp
app.UseMiddleware<SetupRedirectMiddleware>();
```

**What it does:**
1. Checks if application is configured (roles exist, admin account created)
2. If not configured, redirects to `/___setup`
3. Skips static resources, identity pages, setup pages themselves
4. Caches result to avoid repeated DB queries

**Caching logic:**
```csharp
// Static variable keeps state between requests
private static bool? isSetupCompleted = null;

// Lock prevents race conditions during first check
private static readonly object lockObject = new object();

// Checks if database has required data
var rolesExist = await dbContext.Roles.AnyAsync();
var adminExists = await dbContext.Users.Where(u => 
    u.UserRoles.Any(r => r.Role.Name == "Administrators")
).AnyAsync();
```

**Skip conditions:**
- Request to `/___setup` (setup pages)
- Request to `/Identity/*` (login/register)
- Static file request (`.css`, `.js`, `.jpg`, etc.)
- API endpoints

### 6. Exception Handler Middleware

**Purpose:** Catch unhandled exceptions and return error page

```csharp
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/Home/Error");
```

**Development:**
- Shows detailed stack trace
- Shows source code context
- Shows exception chain

**Production:**
- Redirects to `/Home/Error` page
- Generic error message shown to users
- Full error logged server-side

### 7. StaticFiles Middleware

**Purpose:** Serve static resources (CSS, JS, images, etc.)

```csharp
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".js"))
        {
            ctx.Context.Response.Headers
                .Append("Content-Type", "application/javascript");
        }
    }
});
```

**What it does:**
- Matches request to file in `wwwroot/` folder
- Sets appropriate MIME type
- Returns file directly (doesn't go to controllers)
- Short-circuits pipeline if file found

**Short-Circuit:**
```
Request: /css/style.css
  ↓
StaticFiles finds wwwroot/css/style.css
  ↓
Returns file directly ← (Pipeline stops here)
  ↓
Response sent (never reaches controller)
```

### 8. Routing Middleware

**Purpose:** Match request URL to controller/action

```csharp
app.UseRouting();
```

**What it does:**
- Matches URL pattern to route
- Extracts route parameters
- Determines which controller/action to invoke
- Does NOT invoke the action yet

**Example:**
```
Request: /Editor/Pages/edit/123
  ↓
Routing finds: Controller="Pages", Action="Edit", id="123"
  ↓
Continues to next middleware with this info
  ↓
(Action invoked later in Endpoint middleware)
```

### 9. CORS Middleware

**Purpose:** Handle cross-origin requests

```csharp
app.UseCors("AllCors");
```

**What it does:**
- Adds CORS headers to response
- Validates origin if policy configured
- Handles preflight requests

**Note:** SkyCMS has permissive CORS (`AllowAnyOrigin`) for development.

### 10. ResponseCaching Middleware

**Purpose:** Cache responses to reduce load

```csharp
app.UseResponseCaching();
```

**What it does:**
- Checks if response is cacheable
- Returns cached response if available
- Reduces server load for repeated requests

### 11. Authentication Middleware

**Purpose:** Identify the user

```csharp
app.UseAuthentication();
```

**What it does:**
1. Looks for authentication cookie
2. Validates cookie signature
3. Extracts user claims from cookie
4. Sets `HttpContext.User` with user identity

**Flow:**
```
Request with cookie
  ↓
Cookie validated
  ↓
User claims extracted
  ↓
HttpContext.User = ClaimsPrincipal (if valid)
  ↓
Authorization middleware can now check roles
```

**Note:** Authentication happens even for anonymous requests. `HttpContext.User.Identity.IsAuthenticated` is false for anonymous users.

### 12. Authorization Middleware

**Purpose:** Check if user has access to resource

```csharp
app.UseAuthorization();
```

**What it does:**
- Evaluates `[Authorize]` attributes on controller/action
- Checks user roles, claims, policies
- Returns 403 Forbidden if access denied

**Example:**
```csharp
[Authorize(Roles = "Administrators")]
public IActionResult ManageSettings()
{
    // Authorization middleware checks here
    // If user not in "Administrators" role → 403
}
```

### 13. RateLimiter Middleware

**Purpose:** Throttle excessive requests

```csharp
app.UseRateLimiter();
```

**Configuration:**
```csharp
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;              // 4 requests
        options.Window = TimeSpan.FromSeconds(8);  // per 8 seconds
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;               // queue up to 2 more
    }));
```

**Behavior:**
- Allows 4 requests per 8 seconds
- Queue 2 additional requests
- Return 429 Too Many Requests if exceeded

### 14. Endpoint Middleware (Controller Action)

**Purpose:** Execute the actual controller action

Implicit middleware that runs the matched controller/action:

```csharp
public IActionResult GetPage(int id)
{
    // This is the "endpoint"
    // Executes here after all middleware
}
```

## Request and Response Flow

### Full Request Lifecycle

```
HTTP Request arrives
    ↓
[1] ForwardedHeaders: Add proxy info (if applicable)
    ↓
[2] DomainMiddleware: Validate tenant domain (multi-tenant only)
    ↓
[3] DataProtection: Initialize encryption
    ↓
[4] Setup Check: Validate setup wizard access
    ↓
[5] SetupRedirectMiddleware: Check if setup complete
    ↓
[6] Exception Handler: Prepare error handling
    ↓
[7] StaticFiles: Check if static resource
    ├─→ If YES: Return file directly → [Response Flow]
    └─→ If NO: Continue
    ↓
[8] Routing: Match URL to controller/action
    ↓
[9] CORS: Add CORS headers
    ↓
[10] ResponseCaching: Check cache
    ├─→ If cached: Return → [Response Flow]
    └─→ If not: Continue
    ↓
[11] Authentication: Identify user
    ↓
[12] Authorization: Check access
    ├─→ If denied: Return 403 → [Response Flow]
    └─→ If allowed: Continue
    ↓
[13] RateLimiter: Check rate limit
    ├─→ If exceeded: Return 429 → [Response Flow]
    └─→ If allowed: Continue
    ↓
[14] Endpoint (Controller Action): Execute handler
    ↓
    Response generated (View, JSON, etc.)
    ↓
[Response flows back through middleware in reverse order]
    ↓
[13] RateLimiter: Update rate limit
    ↓
[12] Authorization: (No response modification)
    ↓
[11] Authentication: (No response modification)
    ↓
[10] ResponseCaching: Cache response (if applicable)
    ↓
[9] CORS: Ensure CORS headers set
    ↓
[8] Routing: (No response modification)
    ↓
[7] StaticFiles: (No response modification)
    ↓
[6] Exception Handler: (No exception, pass through)
    ↓
[5] SetupRedirectMiddleware: (No response modification)
    ↓
[4] Setup Check: (No response modification)
    ↓
[3] DataProtection: (No response modification)
    ↓
[2] DomainMiddleware: (No response modification)
    ↓
[1] ForwardedHeaders: (No response modification)
    ↓
HTTP Response sent to client
```

## Custom Middleware

### Creating Custom Middleware

To create middleware:

1. **Create the middleware class:**
```csharp
public class CustomMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<CustomMiddleware> logger;

    public CustomMiddleware(RequestDelegate next, ILogger<CustomMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        logger.LogInformation("Before action");
        await next(context);
        logger.LogInformation("After action");
    }
}
```

2. **Register in Program.cs:**
```csharp
app.UseMiddleware<CustomMiddleware>();
```

3. **Position correctly:**
```csharp
// Middleware is executed in registration order
app.UseForwardedHeaders();
app.UseMiddleware<CustomMiddleware>();  // Your middleware here
app.UseRouting();
```

### Middleware vs Filters

| Feature | Middleware | Filter |
|---------|-----------|--------|
| **Scope** | Global, all requests | Per-controller/action |
| **Registration** | Program.cs | Attribute on controller/action |
| **Early access** | Before routing | After routing |
| **Performance** | Checked for all requests | Only matched routes |
| **Use case** | Logging, security, proxies | Authorization, validation |

**Use Middleware for:** Tenant routing, domain validation, request logging  
**Use Filters for:** Per-action authorization, model validation

## Debugging Middleware

### Enable Detailed Logging

```csharp
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddConsole();
```

### Log Middleware Execution

Add logging to track middleware:

```csharp
public async Task InvokeAsync(HttpContext context)
{
    logger.LogInformation($"Middleware executing for {context.Request.Path}");
    try
    {
        await next(context);
        logger.LogInformation($"Middleware completed with status {context.Response.StatusCode}");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Middleware error");
        throw;
    }
}
```

### Trace Request Flow

```bash
# Enable tracing in appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore.Middleware": "Trace"
    }
  }
}
```

## Best Practices

### 1. Middleware Order Matters

- **Authentication** before **Authorization**
- **Routing** before **Endpoint** (always)
- **ForwardedHeaders** first (before auth/routing)
- **Exception handler** before **Static files**

### 2. Keep Middleware Focused

Each middleware should do one thing:
- ✓ DomainMiddleware validates domain
- ✗ Don't log, validate, AND modify request

### 3. Async All the Way

```csharp
// ✓ Correct
public async Task InvokeAsync(HttpContext context)
{
    await next(context);
}

// ✗ Wrong - blocks thread
public void Invoke(HttpContext context)
{
    next(context).Wait();  // DEADLOCK risk!
}
```

### 4. Short-Circuit Carefully

Only short-circuit when appropriate:
```csharp
// ✓ OK - no further processing needed
if (IsStaticFile(context.Request.Path))
    return;  // Don't call next(context)

// ✗ Problem - middleware after this won't run
if (context.Request.Path.StartsWith("/api"))
    return;  // Other middleware (logging, etc.) is skipped
```

### 5. Error Handling

```csharp
try
{
    await next(context);
}
catch (Exception ex)
{
    logger.LogError(ex, "Middleware error");
    // Can still modify response if not sent yet
    if (!context.Response.HasStarted)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Internal error" });
    }
}
```

### 6. Avoid State in Middleware

```csharp
// ✗ Problematic - shared state across requests
public class BadMiddleware
{
    private string userEmail;  // SHARED across all requests!
    
    public async Task InvokeAsync(HttpContext context)
    {
        userEmail = context.User.FindFirst("email")?.Value;  // Race condition!
    }
}

// ✓ Correct - use HttpContext.Items for per-request state
public async Task InvokeAsync(HttpContext context)
{
    var userEmail = context.User.FindFirst("email")?.Value;
    context.Items["UserEmail"] = userEmail;  // Per-request storage
}
```

## Related Documentation

- [Startup Lifecycle](./Startup-Lifecycle.md) - Middleware registration happens here
- [Multi-Tenant Configuration](../Configuration/Multi-Tenant-Configuration.md) - Uses DomainMiddleware
- [Authentication Overview](../Authentication-Overview.md) - Auth middleware details
- [Role-Based Access Control](../Administration/Roles-and-Permissions.md) - Authorization middleware

## Code References

> **Note**: The following source code files are located in the SkyCMS project repository, not in the published documentation.

- **Program.cs**: Lines 430-450+ - Middleware pipeline configuration (`Editor/Program.cs`)
- **SetupRedirectMiddleware**: `Editor/Middleware/SetupRedirectMiddleware.cs`
- **DomainMiddleware**: `Cosmos.ConnectionStrings/DomainMiddleware.cs`
- **TenantValidationMiddleware**: `Cosmos.ConnectionStrings/Middleware/TenantValidationMiddleware.cs`
