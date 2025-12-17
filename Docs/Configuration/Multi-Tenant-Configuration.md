---
title: Multi-Tenant Configuration Guide
description: Set up and manage multiple independent websites on different domains with a single SkyCMS instance
keywords: multi-tenant, architecture, domains, configuration, management
audience: [developers, administrators, architects]
---

# Multi-Tenant Configuration Guide

SkyCMS supports **multi-tenant architecture**, allowing a single instance to serve multiple independent websites on different domains. This guide explains how to set up, configure, and manage multiple tenants in SkyCMS.

## Table of Contents
- [Multi-Tenant Overview](#multi-tenant-overview)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Setting Up Multi-Tenant](#setting-up-multi-tenant)
- [Domain Configuration](#domain-configuration)
- [Tenant Isolation](#tenant-isolation)
- [Managing Multiple Tenants](#managing-multiple-tenants)
- [Shared Editor vs. Decoupled Setup](#shared-editor-vs-decoupled-setup)
- [Best Practices](#best-practices)
- [Troubleshooting Multi-Tenant Issues](#troubleshooting-multi-tenant-issues)

## Multi-Tenant Overview

Multi-tenancy in SkyCMS allows:

- **One application instance** serves **multiple independent websites**
- **Each tenant (domain)** gets its own **database and storage**
- **Automatic tenant routing** based on request domain
- **Complete data isolation** between tenants
- **Shared infrastructure** for cost efficiency

### When to Use Multi-Tenant

✓ **Use multi-tenant if:**
- Hosting multiple sites on shared infrastructure
- Managing multiple client websites from one application
- Need cost-efficient resource sharing
- Each site has separate data requirements

✗ **Avoid multi-tenant for:**
- Single website deployments (use single-tenant setup instead)
- Highly customized per-site configurations
- Extremely large scale (1000+ tenants)

## Architecture

### Components

```
┌─────────────────────────────────────────────────┐
│        SkyCMS Editor Application Instance        │
│ (Single running instance serves all domains)    │
├─────────────────────────────────────────────────┤
│                                                 │
│  ┌─────────────────────────────────────────┐   │
│  │   Dynamic Configuration Layer            │   │
│  │  (TenantContext + Middleware)            │   │
│  └─────────────────────────────────────────┘   │
│                     │                           │
│       ┌─────────────┼─────────────┐             │
│       ↓             ↓             ↓             │
│   tenant1       tenant2       tenant3           │
│   Database      Database      Database          │
│   Storage       Storage       Storage           │
└─────────────────────────────────────────────────┘
```

### Key Components

| Component | Purpose |
|-----------|---------|
| **DomainMiddleware** | Extracts domain from HTTP request and validates tenant |
| **TenantContext** | Maintains current tenant in async context (AsyncLocal) |
| **IDynamicConfigurationProvider** | Resolves connection strings per domain |
| **DynamicConfigDbContext** | Stores tenant metadata (connections and metrics) |
| **Connection** | Maps domain → database & storage connection strings |

## Prerequisites

Before setting up multi-tenant, ensure:

1. **Central Configuration Database**: A database for storing tenant connection information
   - Can be any supported SkyCMS database (SQL Server, Cosmos DB, MySQL, SQLite)
   - Stores the `Connection` table with domain-to-connection mappings
   - Stores `Metrics` table for per-tenant usage tracking

2. **Per-Tenant Databases**: Each tenant needs its own database
   - Can use different database types per tenant if needed
   - Located independently from central config database

3. **Per-Tenant Storage**: Each tenant needs blob storage
   - Can be S3, Azure Blob, Cloudflare R2, Google Cloud Storage, etc.
   - Located independently from other tenants

4. **DNS Configuration**: Domain names pointing to your SkyCMS instance
   - All domains resolve to the same IP/load balancer
   - SkyCMS determines which tenant based on request domain

## Setting Up Multi-Tenant

### Step 1: Prepare Central Configuration Database

The central configuration database stores tenant metadata.

1. Create a database using your preferred provider
2. Record the **connection string** for later use
3. Example (SQL Server):
   ```
   Server=myserver.database.windows.net;Database=skycms-config;User Id=admin;Password=SecurePassword123;
   ```

### Step 2: Configure Environment Variables

Set these environment variables **before** launching SkyCMS:

```powershell
# Central Configuration Database
$env:CONNECTIONSTRINGS_CONFIGDB = "your-config-db-connection-string"

# Note: Do NOT use the wizard with multi-tenant
# The setup wizard is not supported for multi-tenant setups
```

**Important**: For multi-tenant, the Setup Wizard is **not available**. All configuration must be done via environment variables or by directly populating the central configuration database.

### Step 3: Populate Tenant Configuration

Before requests arrive, populate the `Connections` table with tenant information. You can do this via:

#### Option A: Direct Database Insertion

Connect to your central configuration database and insert a row into the `Connections` table:

```sql
INSERT INTO Connections (
    Id, 
    AllowSetup, 
    DomainNames, 
    DbConn, 
    StorageConn, 
    Customer, 
    ResourceGroup, 
    PublisherMode, 
    BlobPublicUrl, 
    MicrosoftAppId, 
    PublisherRequiresAuthentication, 
    WebsiteUrl
)
VALUES (
    NEWID(),                          -- Id
    1,                                -- AllowSetup (1 = true for initial setup)
    '[tenant1.example.com]',          -- DomainNames (JSON array)
    'connection-string-db',           -- DbConn
    'connection-string-storage',      -- StorageConn
    'My Company',                     -- Customer
    'myresourcegroup',                -- ResourceGroup
    'Static',                         -- PublisherMode
    'https://storage.blob.core.windows.net/container/', -- BlobPublicUrl
    '',                               -- MicrosoftAppId
    0,                                -- PublisherRequiresAuthentication
    'https://tenant1.example.com'    -- WebsiteUrl
)
```

#### Option B: Admin Interface (After First Request)

1. Launch SkyCMS
2. Create initial administrator account via first-run setup
3. Navigate to **Administration → Tenants** (if available in UI)
4. Add tenant configurations

### Step 4: Deploy and Start Application

```bash
docker run -e CONNECTIONSTRINGS_CONFIGDB="your-config-db-connection-string" \
           -p 80:8080 \
           toiyabe/sky-editor:latest
```

### Step 5: Verify Multi-Tenant Setup

1. Access `https://tenant1.example.com/___setup`
2. Follow the setup wizard for the first tenant
3. Access `https://tenant2.example.com/___setup`
4. Follow the setup wizard for the second tenant
5. Verify each tenant has independent content and configuration

## Domain Configuration

### Domain-to-Tenant Mapping

Each domain must map to a `Connection` record containing connection strings.

### Single Domain per Tenant

```
Connection Record:
├── DomainNames: ["tenant1.example.com"]
├── DbConn: "connection-string-tenant1-db"
└── StorageConn: "connection-string-tenant1-storage"
```

### Multiple Domains per Tenant (Aliases)

A single tenant can respond to multiple domains:

```
Connection Record:
├── DomainNames: ["tenant1.example.com", "tenant1-staging.example.com", "www.tenant1.example.com"]
├── DbConn: "connection-string-tenant1-db"
└── StorageConn: "connection-string-tenant1-storage"
```

All three domains route to the same database and storage.

### Domain Validation

When a request arrives:
1. SkyCMS extracts the **host domain** from the request
2. **DomainMiddleware** checks if domain exists in `Connections` table
3. If **valid**: Continues processing with that tenant's context
4. If **invalid**: Returns HTTP 404 with "Tenant not found" message

### Adding New Domains

To add a new domain to an existing tenant:

1. Update the `DomainNames` array in the `Connection` record
2. Examples:
   - From: `["tenant1.example.com"]`
   - To: `["tenant1.example.com", "www.tenant1.example.com"]`

## Tenant Isolation

### How Isolation Works

```csharp
// 1. Request arrives for "tenant1.example.com"
// 2. DomainMiddleware extracts domain
// 3. TenantContext is set to "tenant1.example.com"
// 4. Database queries use tenant1's database connection
// 5. Storage operations use tenant1's storage connection
// 6. TenantContext is cleared after response
```

### TenantContext (AsyncLocal)

`TenantContext` maintains tenant information across async operations:

```csharp
// Access current tenant
var currentTenant = TenantContext.CurrentDomain;  // "tenant1.example.com"

// Check if tenant context is set
if (TenantContext.HasContext)
{
    // Tenant-specific logic
}

// Execute within tenant context (for background jobs)
await TenantContext.ExecuteAsync("tenant1.example.com", async () =>
{
    // All database/storage operations use tenant1's connections
    var pages = await pageService.GetAllAsync();
});
```

### Data Isolation Guarantees

- **Database**: Each tenant's queries only access their database
- **Storage**: Each tenant's files stored in their storage account
- **Authentication**: Each tenant's users isolated to their database
- **Configuration**: Each tenant's settings isolated

### Cross-Tenant Concerns

**Important**: Developer code must respect tenant isolation:

✓ **OK - Automatically isolated:**
```csharp
// Uses current TenantContext
var pages = await dbContext.Pages.ToListAsync();
```

✗ **Dangerous - Manual domain bypass:**
```csharp
// Switching tenant without clearing context can leak data
TenantContext.CurrentDomain = "tenant2.example.com";
var otherTenantPages = await dbContext.Pages.ToListAsync();
TenantContext.Clear();
```

## Managing Multiple Tenants

### Adding a New Tenant

1. **Create tenant infrastructure**:
   - Database for tenant
   - Storage account/bucket
   - Note connection strings

2. **Create Connection record** in central database:
   ```sql
   INSERT INTO Connections ...
   ```

3. **Point domain** DNS to SkyCMS instance

4. **Initialize tenant**: Access `https://newtenant.com/___setup` and complete wizard

### Removing a Tenant

1. **Update DNS**: Point domain away from SkyCMS
2. **Delete Connection record** from central database
   ```sql
   DELETE FROM Connections WHERE DomainNames LIKE '%newtenant.com%'
   ```
3. **Archive tenant data** (database backup before deletion)
4. **Optional**: Delete tenant database and storage

### Modifying Tenant Configuration

To update a tenant's connection strings:

```sql
UPDATE Connections
SET DbConn = 'new-connection-string'
WHERE DomainNames LIKE '%tenant1.example.com%'
```

Changes take effect on next request. No restart required (uses caching with refresh).

### Monitoring Multi-Tenant Usage

The `Metrics` table tracks per-tenant resource usage:

```sql
SELECT ConnectionId, TimeStamp, BlobStorageBytes, DatabaseRuUsage
FROM Metrics
WHERE ConnectionId = 'tenant1-id'
ORDER BY TimeStamp DESC
LIMIT 30
```

Track metrics across tenants for:
- Storage usage per tenant
- Database RU consumption
- Bandwidth usage
- Capacity planning

## Shared Editor vs. Decoupled Setup

### Shared Editor (Recommended for Multi-Tenant)

Single SkyCMS Editor instance manages **all tenants**:

```
┌──────────────────────────┐
│   Single SkyCMS Editor   │
│   Serves all domains     │
└──────────────────────────┘
         ↓
    ┌────┼────┐
    ↓    ↓    ↓
 Tenant1 Tenant2 Tenant3
  Pages   Pages   Pages
```

**Advantages:**
- Single application to maintain
- Lower hosting costs
- Centralized content management
- Tenant-specific publishers per site

**Disadvantages:**
- All tenants share compute resources
- Single point of failure
- Configuration management complexity

### Decoupled (Multiple Editor Instances)

Separate SkyCMS Editor instance **per tenant**:

```
┌──────────────────────────┐
│  Editor 1    Editor 2    │
│  (Tenant1)   (Tenant2)   │
└──────────────────────────┘
     ↓             ↓
  Tenant1       Tenant2
  Pages         Pages
```

**Advantages:**
- Complete isolation
- Independent scaling
- Simpler per-tenant configuration
- Better for large tenants

**Disadvantages:**
- Multiple applications to maintain
- Higher hosting costs
- Duplicate infrastructure
- No shared context

## Best Practices

### 1. Database Strategy

- **Shared Config DB**: Always use centralized configuration database
- **Separate Tenant DBs**: Each tenant gets isolated database
- **Consider DB Type**: Can use different DB providers per tenant
- **Backup Strategy**: Backup each tenant's database independently

### 2. Storage Strategy

- **Separate Storage**: Each tenant gets dedicated storage account
- **Cost Monitoring**: Track per-tenant storage usage via Metrics
- **CDN Configuration**: Each tenant can have own CDN if needed
- **Backup Plan**: Regular backups per tenant

### 3. Domain Management

- **DNS Planning**: Plan domain strategy before setup
- **Use Aliases**: Support multiple domains per tenant if needed
- **SSL Certificates**: Use wildcard cert or SAN cert for all domains
- **Subdomain Strategy**: Consider using subdomains vs. separate domains

### 4. Setup Wizard Limitations

- **Multi-tenant incompatible**: Cannot use setup wizard for centralized multi-tenant
- **Per-tenant setup**: Each domain can complete setup independently
- **First tenant special**: First tenant initialized as "bootstrap" tenant

### 5. Performance Optimization

- **Connection Pooling**: Each connection string gets its own pool
- **Caching**: Cache connection strings to avoid repeated lookups
- **Async Context**: Use TenantContext.ExecuteAsync for background jobs
- **Metrics**: Monitor per-tenant performance via Metrics table

### 6. Security Considerations

- **Encryption**: Encrypt connection strings at rest
- **Access Control**: Restrict admin access to Connection table
- **Domain Validation**: Always validate domain before routing
- **Audit Logging**: Log all multi-tenant configuration changes

### 7. Testing Multi-Tenant

Test with realistic tenant scenarios:

```csharp
// Test isolation
var tenant1Pages = await GetPagesForTenant("tenant1.example.com");
var tenant2Pages = await GetPagesForTenant("tenant2.example.com");
Assert.NotEqual(tenant1Pages, tenant2Pages);

// Test domain switching
await TenantContext.ExecuteAsync("tenant1.example.com", async () =>
{
    var pages = await dbContext.Pages.ToListAsync();
    Assert.All(pages, p => Assert.Equal("tenant1", p.TenantId));
});

// Test invalid domain
Assert.Throws<InvalidOperationException>(() =>
{
    return GetPagesForTenant("invalid.example.com");
});
```

## Troubleshooting Multi-Tenant Issues

### "Tenant Not Found" Error

**Symptoms**: HTTP 404 when accessing tenant domain

**Resolution**:
1. Verify domain DNS resolves to SkyCMS instance
2. Check `Connections` table has entry for domain
3. Ensure `DomainNames` JSON array contains exact domain (case-insensitive)
4. Review middleware logs for domain validation details

### Data Appearing in Wrong Tenant

**Symptoms**: Tenant1's content visible when accessing Tenant2

**Resolution**:
1. Verify `TenantContext` is being cleared after requests
2. Check database connection strings are unique per tenant
3. Verify middleware is extracting correct domain
4. Check for hardcoded tenant assumptions in code
5. Review for AsyncLocal context leaks across requests

### Setup Wizard Not Available

**Symptoms**: Cannot access `/___setup` for multi-tenant

**Resolution**:
- **Expected behavior**: Setup wizard is disabled for multi-tenant
- **Workaround**: Use environment variables or direct DB insertion
- For single-tenant setups, wizard is available

### Slow Domain Lookups

**Symptoms**: High latency on first request to new domain

**Resolution**:
1. Enable connection caching in configuration provider
2. Preload all connections at startup:
   ```csharp
   var provider = services.GetRequiredService<IDynamicConfigurationProvider>();
   await provider.PreloadAllConnectionsAsync();
   ```
3. Consider Redis caching layer for connection strings
4. Monitor database query performance

### Tenant Can't Connect to Storage

**Symptoms**: File upload fails; storage operations fail

**Resolution**:
1. Verify storage connection string in `Connections.StorageConn`
2. Check storage account credentials are valid
3. Verify storage account has proper CORS configuration
4. Test connection string independently
5. Check firewall/network access to storage service

### Mixed Domain Names in Connection

**Symptoms**: Some domains work, others show 404

**Resolution**:
1. Verify all domain names in `DomainNames` JSON array
2. Ensure exact domain name match (case-insensitive normally)
3. Check no leading/trailing spaces in domain names
4. Example correct format:
   ```json
   ["tenant1.example.com", "www.tenant1.example.com"]
   ```

## Related Documentation

- [Configuration Overview](./README.md) - All configuration options
- [Database Configuration](./Database-Overview.md) - Database setup per tenant
- [Storage Configuration](./Storage-Overview.md) - Storage setup per tenant
- [Installation Overview](../Installation/README.md) - General installation
- [Minimum Required Settings](../Installation/MinimumRequiredSettings.md) - Multi-tenant requirements

## Code References

- **Tenant Context**: [TenantContext.cs](../../Cosmos.ConnectionStrings/TenantContext.cs)
- **Domain Middleware**: [DomainMiddleware.cs](../../Cosmos.ConnectionStrings/DomainMiddleware.cs)
- **Tenant Validation**: [TenantValidationMiddleware.cs](../../Cosmos.ConnectionStrings/TenantValidationMiddleware.cs)
- **Configuration Provider**: [IDynamicConfigurationProvider.cs](../../Cosmos.ConnectionStrings/IDynamicConfigurationProvider.cs)
- **Connection Model**: [Connection.cs](../../Cosmos.ConnectionStrings/Connection.cs)
- **Config Database**: [DynamicConfigDbContext.cs](../../Cosmos.ConnectionStrings/DynamicConfigDbContext.cs)
