---
title: AspNetCore.Identity.FlexDb Component
description: Flexible database provider for ASP.NET Core Identity supporting multiple database backends
keywords: ASP.NET-Core, Identity, database, provider, component, authentication
audience: [developers, architects]
---

# AspNetCore.Identity.FlexDb - Flexible Database Provider for ASP.NET Core Identity

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-GPL%203.0-blue.svg)](https://www.gnu.org/licenses/gpl-3.0.html)
[NuGet package listing coming soon]

A flexible, multi-database implementation of ASP.NET Core Identity that **automatically selects the appropriate database provider** based on your connection string. Supports Azure Cosmos DB, SQL Server, and MySQL with seamless switching between providers.

---

## Table of Contents

- [What's New](#whats-new)
- [Overview](#overview)
- [Supported Database Providers](#supported-database-providers)
- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Configuration](#configuration)
- [Security Features](#security-features)
- [Advanced Usage](#advanced-usage)
- [Extending FlexDb](#extending-flexdb)
- [Performance](#performance-and-scaling)
- [Migration Guide](#migration-guide)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)

---

## What's New

### Version 9.0+

- **.NET 9** support with C# 13.0 features
- **Enhanced Strategy Pattern** with improved provider detection
- **Thread-safe** stateless strategy implementations
- **Improved documentation** with comprehensive XML comments
- **Performance optimizations** for all providers
- **Better error messages** with detailed provider information

---

## Overview

AspNetCore.Identity.FlexDb **eliminates the need to choose a specific database provider at compile time**. Simply provide a connection string, and the library automatically configures the correct Entity Framework provider using the **Strategy Pattern**.

### Why FlexDb?

- **Zero Code Changes**: Switch databases by changing connection strings only
- **Rapid Development**: No provider-specific configuration needed
- **Multi-Environment**: Use MySQL for dev, SQL Server for staging, Cosmos DB for production
- **Single Package**: All providers in one NuGet package
- **Extensible**: Add custom providers by implementing `IDatabaseConfigurationStrategy`
- **Secure**: Built-in personal data encryption and protection

### Key Features

| Feature | Description |
|---------|-------------|
| **Automatic Provider Detection** | Intelligently selects database provider from connection string patterns |
| **Multi-Database Support** | Cosmos DB, SQL Server, and MySQL out of the box |
| **Strategy Pattern** | Clean, extensible architecture for adding new providers |
| **Azure Integration** | Native support for Azure Cosmos DB and Azure SQL Database |
| **Backward Compatibility** | Supports legacy Cosmos DB configurations |
| **Personal Data Protection** | Built-in encryption for sensitive user data |
| **Thread-Safe** | All operations are thread-safe and concurrent-friendly |
| **Well Documented** | Comprehensive XML documentation for all public APIs |

---

## Supported Database Providers

### Azure Cosmos DB

**Best for:** Global-scale, cloud-native applications requiring low latency and high availability

**Optimizations:**
- Optimized partition key strategy for user data
- Minimized RU consumption
- Efficient batch operations

### SQL Server / Azure SQL Database

**Best for:** Enterprise applications requiring ACID compliance and relational integrity

**Features:**
- Full ACID compliance, advanced indexing, enterprise features
- Connection Pooling
- Query Optimization

### MySQL

**Best for:** Open-source projects, cost-effective hosting, LAMP stack integration

**Features:**
- Open source, wide hosting support, good performance
- Ideal for Linux hosting, open-source projects

---

## Quick Start

### Installation

Install the NuGet package:

```bash
dotnet add package AspNetCore.Identity.FlexDb
```

### Basic Configuration (Minimal hosting, .NET 9)

```csharp
using AspNetCore.Identity.FlexDb;
using AspNetCore.Identity.FlexDb.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Connection string determines the provider automatically
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    CosmosDbOptionsBuilder.ConfigureDbOptions(options, connectionString));

builder.Services.AddCosmosIdentity<ApplicationDbContext, IdentityUser, IdentityRole, string>(
    options =>
    {
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    },
    cookieExpireTimeSpan: TimeSpan.FromDays(30),
    slidingExpiration: true
);

var app = builder.Build();

app.MapGet("/healthz", () => Results.Ok("ok"));
app.Run();

public class ApplicationDbContext : CosmosIdentityDbContext<IdentityUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions options) : base(options) { }
}
```

### Connection String Examples

```json
{
  "ConnectionStrings": {
    "CosmosDb": "AccountEndpoint=https://myaccount.documents.azure.com:443/;AccountKey=mykey;Database=MyDatabase;",
    "SqlServer": "Server=tcp:myserver.database.windows.net,1433;Initial Catalog=MyDatabase;User ID=myuser;Password=mypassword;",
    "MySQL": "Server=myserver;Port=3306;uid=myuser;pwd=mypassword;database=MyDatabase;"
  }
}
```

---

## Security Features

### Data Protection

- **Personal Data Encryption**: Automatic encryption of PII fields
- **Secure Key Management**: Integration with ASP.NET Core Data Protection
- **Provider-Agnostic**: Works across all supported database types

### Authentication

- **Cookie Authentication**: Configurable expiration and sliding windows
- **Two-Factor Authentication**: Built-in 2FA support
- **External Providers**: OAuth integration ready

### Authorization

- **Role-Based Access**: Traditional role management
- **Claims-Based Security**: Fine-grained permission system
- **Policy-Based Authorization**: Flexible authorization policies

---

## Architecture

FlexDb uses a **Strategy Pattern** to choose the database provider at runtime. Each provider implements `IDatabaseConfigurationStrategy`, ensuring consistent behavior while allowing provider-specific optimizations.

## Configuration

- **Package ID**: `AspNetCore.Identity.FlexDb`
- **Target Framework**: .NET 9.0
- **Repository**: [GitHub](https://github.com/CWALabs/SkyCMS)
- **License**: MIT License
- **Dependencies**:
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore
  - Microsoft.EntityFrameworkCore.Cosmos
  - Microsoft.EntityFrameworkCore.SqlServer
  - MySql.EntityFrameworkCore

## Advanced Usage

- Override default strategies by implementing `IDatabaseConfigurationStrategy`.
- Use separate connection strings per environment to change providers without code changes.

## Extending FlexDb

- Add a new provider by creating a strategy that inspects the connection string and configures Entity Framework accordingly.
- Register the custom strategy via dependency injection so it participates in provider selection.

## Performance and Scaling

- Keep connection strings specific to workloads to avoid hot partitions (for Cosmos DB).
- Use async APIs for higher throughput and enable connection pooling where applicable.

## Migration Guide

- Replace provider-specific services with FlexDb registration.
- Move connection strings to configuration and validate provider detection in lower environments first.

## Troubleshooting

- Enable logging for `CosmosDbOptionsBuilder` and `IDatabaseConfigurationStrategy` selections to verify provider detection.
- Verify the connection string matches expected patterns for the intended provider.

## Contributing

- Contributions are welcome. Please open an issue to discuss proposed changes before submitting a PR.

---

## License

This project is licensed under the MIT License. See the license file for details.

## 🔗 Related Projects

This identity provider is used by the following SkyCMS components (see source repository for details):

- **SkyCMS**: Content management system using this identity provider
- **Editor**: Content editing interface
- **Publisher**: Public website engine

> **Note**: For comprehensive component documentation, see the source code repository at [GitHub](https://github.com/CWALabs/SkyCMS).

