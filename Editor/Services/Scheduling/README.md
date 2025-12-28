# Background Job Services (Hangfire)

## Important: ApplicationDbContext in Background Jobs

?? **CRITICAL**: Background jobs (Hangfire workers) run on **worker threads without HTTP context**. 

### The Problem

`ApplicationDbContext` is registered as a **Scoped** service with a factory that requires HTTP context to resolve the tenant domain. This works perfectly for web requests but **fails in Hangfire background jobs** because there's no HTTP request/context available.

### The Solution

**DO NOT** resolve `ApplicationDbContext` from the DI container in background jobs:

```csharp
// ? WRONG - This will throw InvalidOperationException
var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
```

**DO** create `ApplicationDbContext` instances directly:

```csharp
// ? CORRECT - Single-tenant mode
var configuration = serviceProvider.GetRequiredService<IConfiguration>();
var connectionString = configuration.GetConnectionString("ApplicationDbContextConnection");
using var dbContext = new ApplicationDbContext(connectionString);

// ? CORRECT - Multi-tenant mode
var connection = await configurationProvider.GetTenantConnectionAsync(domainName);
using var dbContext = new ApplicationDbContext(connection.DbConn);
```

### Example: ArticleScheduler

See `ArticleScheduler.RunForTenant()` for the correct implementation pattern:

- **Multi-tenant mode**: Creates `ApplicationDbContext` directly using the connection from `IDynamicConfigurationProvider`
- **Single-tenant mode**: Creates `ApplicationDbContext` directly using the connection string from `IConfiguration`

### Why This Design?

1. **Web Requests**: Need dynamic tenant resolution based on the incoming HTTP request (domain/headers)
2. **Background Jobs**: Process specific tenants explicitly, so they know the connection string upfront
3. **Separation of Concerns**: Background jobs shouldn't depend on HTTP-specific infrastructure

### Other Affected Services

Any service injected into Hangfire jobs that depends on `ApplicationDbContext` must also follow this pattern. This includes:

- `IStorageContext` / `StorageContext` - Create directly with connection string
- Any scoped services that internally use `ApplicationDbContext`

### Testing Background Jobs

When writing unit tests for Hangfire jobs:

1. Don't mock `IServiceProvider` to return `ApplicationDbContext` 
2. Instead, create test instances directly with a test connection string
3. Use in-memory databases or test database containers for integration tests
