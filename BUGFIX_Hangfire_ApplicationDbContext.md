# Fix: ApplicationDbContext Resolution in Hangfire Background Jobs

## Problem Summary

**Exception**: `System.InvalidOperationException: Unable to determine tenant domain for ApplicationDbContext resolution`

**Location**: Hangfire worker thread executing `ArticleScheduler.ExecuteAsync()`

**Root Cause**: 
- `ApplicationDbContext` is registered as a Scoped service with a factory that requires HTTP context
- Hangfire background jobs run on worker threads **without HTTP context**
- Single-tenant mode tried to resolve `ApplicationDbContext` from DI, which triggered the factory
- The factory failed because `httpContextAccessor.HttpContext` was `null`

## Changes Made

### 1. Fixed ArticleScheduler.cs

**File**: `Editor/Services/Scheduling/ArticleScheduler.cs`

**Change**: Modified `RunForTenant()` method in single-tenant mode to create `ApplicationDbContext` directly instead of resolving from DI:

```csharp
// OLD (BROKEN):
var dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();

// NEW (FIXED):
var configuration = scopedServices.GetRequiredService<IConfiguration>();
var connectionString = configuration.GetConnectionString("ApplicationDbContextConnection");
using (var dbContext = new ApplicationDbContext(connectionString))
{
    // ... use dbContext
}
```

This matches the multi-tenant pattern which was already correct.

### 2. Enhanced Error Messages in MultiTenant.cs

**File**: `Editor/Boot/MultiTenant.cs`

**Change**: Added better error messaging to the `ApplicationDbContext` factory to help diagnose issues:

- Detects when HTTP context is `null` (background job scenario)
- Provides specific guidance on how to fix the issue
- Helps distinguish between background job issues vs. middleware configuration issues

### 3. Added Documentation

**File**: `Editor/Services/Scheduling/README.md`

**Purpose**: Developer guide explaining:
- Why background jobs can't use scoped `ApplicationDbContext` from DI
- How to correctly create `ApplicationDbContext` in background jobs
- Examples for both single-tenant and multi-tenant modes
- Testing considerations

## Verification

? **Multi-tenant mode**: Already working correctly (creates context directly)
? **Single-tenant mode**: Now fixed (creates context directly)  
? **Error messages**: Now provide actionable guidance
? **Documentation**: Prevents future occurrences

## Testing Recommendations

1. **Verify single-tenant scheduler**: Run the application in single-tenant mode and wait for the Hangfire job to execute (every 10 minutes, or trigger manually via Hangfire dashboard)

2. **Verify multi-tenant scheduler**: Run in multi-tenant mode and verify each tenant's jobs execute without errors

3. **Test error message**: Try to inject `ApplicationDbContext` into a Hangfire job to verify the improved error message appears

## Impact

- **Breaking Changes**: None
- **Behavior Changes**: Single-tenant Hangfire jobs will now work correctly
- **Performance**: No impact (same code path, just instantiation method changed)
- **Compatibility**: Fully backward compatible

## Related Issues

This pattern must be followed for **any** Hangfire job that needs database access:

1. Don't inject scoped services that depend on HTTP context
2. Create `ApplicationDbContext` directly with connection string
3. Create `StorageContext` directly with connection string  
4. Use `IServiceProvider.CreateScope()` for other services that are safe to resolve

## Future Considerations

If more background jobs are added, consider creating a base class or helper that encapsulates the pattern of creating properly scoped database contexts for both single and multi-tenant modes.
