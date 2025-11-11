# SkyCMS Developer Documentation

Welcome to the developer documentation area. This section complements the feature guides under `Docs/` by describing the architecture, shared components, and extension points used across SkyCMS projects.

## Overview

SkyCMS is a cloud-native content management system built on .NET 9.0 with a modular architecture that separates concerns across multiple projects. The system supports multiple database providers (Cosmos DB, SQL Server, MySQL, SQLite) and multiple storage providers (Azure Blob Storage, Amazon S3, Cloudflare R2).

## Architecture

### Core Projects

- **Sky.Editor** (`Editor/`): Content authoring and administration application
- **Sky.Publisher** (`Publisher/`): Public-facing website renderer and content delivery
- **Cosmos.Common** (`Common/`): Shared library with business logic, data models, and base controllers
- **Cosmos.BlobService** (`Cosmos.BlobService/`): Multi-cloud storage abstraction layer
- **Cosmos.DynamicConfig** (`Cosmos.ConnectionStrings/`): Multi-tenant dynamic configuration management
- **AspNetCore.Identity.FlexDb** (`AspNetCore.Identity.FlexDb/`): Flexible multi-database identity provider

### Technology Stack

- **Runtime**: .NET 9.0, ASP.NET Core
- **Data Access**: Entity Framework Core 9.0 with multi-provider support
- **Authentication**: ASP.NET Core Identity with external providers (Google, Microsoft)
- **Frontend**: Bootstrap 5, CKEditor 5, GrapesJS, Monaco Editor, FilePond
- **Storage**: Azure Blob Storage, AWS S3, Cloudflare R2 (via abstraction layer)
- **Databases**: Azure Cosmos DB, SQL Server, MySQL, SQLite

## Sections

- **Controllers**
  - [HomeControllerBase](Controllers/HomeControllerBase.md): Shared base controller used by Publisher and Editor
  - [PubControllerBase](Controllers/PubControllerBase.md): Secure file access controller for Publisher

- **Editor Widgets**
  - [Image Widget](ImageWidget.md): Per‑widget attributes (including `data-ccms-enable-alt-editor`) and developer hooks

## Key Concepts

### Multi-Database Support

SkyCMS automatically detects and configures the appropriate database provider based on connection string patterns:

- **Cosmos DB**: `AccountEndpoint=...`
- **SQL Server**: `Server=...` or `Data Source=...`
- **MySQL**: `server=...` (lowercase)
- **SQLite**: `Data Source=...` with `.db` extension

See [DatabaseConfig.md](../DatabaseConfig.md) for detailed configuration.

### Multi-Cloud Storage

The Cosmos.BlobService provides a unified interface for multiple storage providers:

- **Azure Blob Storage**: `DefaultEndpointsProtocol=https;AccountName=...`
- **Amazon S3**: `Bucket=...;Region=...;KeyId=...;Key=...`
- **Cloudflare R2**: `AccountId=...;Bucket=...;KeyId=...;Key=...`

See [StorageConfig.md](../StorageConfig.md) for detailed configuration.

### Multi-Tenancy

The Cosmos.DynamicConfig project enables multi-tenant configurations where a single application instance can serve multiple customers with isolated databases and storage. See the [Cosmos.DynamicConfig README](../../Cosmos.ConnectionStrings/README.md) for details.

## Extending SkyCMS

### Adding Custom Controllers

Extend base controllers for common functionality:

```csharp
public class MyController : HomeControllerBase
{
    public MyController(
        ArticleLogic articleLogic,
        ApplicationDbContext dbContext,
        StorageContext storageContext,
        ILogger<MyController> logger,
        IEmailSender emailSender)
        : base(articleLogic, dbContext, storageContext, logger, emailSender)
    {
    }
}
```

### Custom Database Providers

To add a new database provider, implement a strategy in AspNetCore.Identity.FlexDb:

1. Create a class implementing `IDatabaseConfigurationStrategy`
2. Define connection string detection logic
3. Configure EF Core options for the provider
4. Register the strategy with appropriate priority

### Custom Storage Providers

To add a new storage provider to Cosmos.BlobService:

1. Implement the `ICosmosStorage` interface
2. Add detection logic in `StorageContext`
3. Handle provider-specific operations
4. Test with multi-tenant scenarios

## Development Guidelines

### Code Standards

- Follow .NET coding conventions
- Use StyleCop for code style enforcement
- Add XML documentation for public APIs
- Write unit tests for new functionality
- Use async/await for I/O operations

### Configuration

- Use `appsettings.json` for single-tenant setups
- Use Cosmos.DynamicConfig for multi-tenant deployments
- Never commit secrets to source control
- Use Azure Key Vault or User Secrets for sensitive data

### Performance

- Use caching where appropriate (memory cache for configuration)
- Optimize database queries (avoid N+1 problems)
- Use streaming for large file operations
- Consider CDN integration for static assets

## Related Documentation

- **Project READMEs**: Each project has detailed documentation in its folder
- **Configuration Guides**: [Database](../DatabaseConfig.md) | [Storage](../StorageConfig.md)
- **Deployment**: [Azure Installation](../AzureInstall.md) | [AWS CloudFormation](../../AWS/README.md)
- **User Guides**: [Templates](../Templates/Readme.md) | [Editors](../Editors/) | [File Management](../FileManagement/)

> Tip: If you introduce new shared services or base classes, add corresponding docs here to keep the codebase discoverable.

Last updated: November 2025 (.NET 9.0 update)

