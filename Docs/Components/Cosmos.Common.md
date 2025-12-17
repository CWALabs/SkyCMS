---
title: Cosmos.Common Component
description: SkyCMS core library providing shared functionality for Editor and Publisher applications
keywords: component, common, core-library, shared, utilities
audience: [developers, architects]
---

# Cosmos.Common - SkyCMS Core Library

> **Package Info:** This is the `Cosmos.Common` package with namespace `Cosmos.Common.*`. It provides shared functionality for both Sky.Editor and Cosmos.Publisher applications.

## Overview

Cosmos.Common is the foundational library for the SkyCMS content management system, providing core functionality, data models, services, and base controllers that are shared across both the Editor and Publisher applications. This package contains essential components for content management, authentication, data access, and utility functions.

## What's New

- Updated docs aligned with .NET 9 and the latest storage/database guides
- Cross-links to Storage and Database configuration, AWS S3, and Cloudflare R2 setup

## Features

### Core Infrastructure

- **Multi-Database Support**: Entity Framework integration with Cosmos DB, SQL Server, and MySQL
- **Base Controllers**: Common controller functionality for Editor and Publisher applications
- **Data Models**: Comprehensive set of entities for content management
- **Utility Functions**: Essential helper methods and extensions
- **Authentication Integration**: ASP.NET Core Identity with flexible provider support

### Content Management

- **Article Management**: Complete article lifecycle management with versioning
- **Page Publishing**: Published page management and routing
- **Layout System**: Template and layout management for content presentation
- **File Management**: Integration with blob storage for media handling
- **Search Functionality**: Content search and indexing capabilities

### Multi-Tenant Architecture

- **Contact Management**: Customer contact and communication handling
- **Metrics Collection**: Usage tracking and analytics
- **Security**: Role-based access control and article permissions
- **Configuration Management**: Dynamic settings and configuration

### Developer Tools

- **Health Checks**: Database connectivity and system status monitoring
- **Logging**: Comprehensive activity logging and audit trails
- **Cache Management**: Memory caching with Cosmos DB integration
- **Validation**: Model validation and data integrity

## Installation

This package is part of the SkyCMS solution and can be obtained by cloning the [SkyCMS GitHub repository](https://github.com/CWALabs/SkyCMS).

### NuGet Package

The package is also available on NuGet:

```bash
Install-Package Cosmos.Common
```

Or via .NET CLI:

```bash
dotnet add package Cosmos.Common
```

## Configuration

### Database Configuration

SkyCMS registers `ApplicationDbContext` in the host app. In the provided templates, the connection string key is `ConnectionStrings:ApplicationDbContextConnection` (with `DefaultConnection` as a common fallback). Choose the EF Core provider accordingly.

#### Cosmos DB

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "AccountEndpoint=https://your-cosmos-account.documents.azure.com:443/;AccountKey=your-key;Database=your-database"
  }
}
```

#### SQL Server

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=your-database;Trusted_Connection=true;"
  }
}
```

#### MySQL

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=your-server;database=your-database;user=your-user;password=your-password"
  }
}
```

## Key Components

### Core Entities

| Entity | Description |
|--------|-------------|
| `Article` | Main content articles with versioning |
| `PublishedPage` | Published pages accessible via URLs |
| `Layout` | Page layouts and templates |
| `Template` | Reusable content templates |
| `CatalogEntry` | Article catalog with permissions |
| `Contact` | Customer contact information |
| `Setting` | System configuration settings |

### Base Controllers

- **HomeControllerBase**: Common functionality for home controllers
- **PubControllerBase**: Secure file access and authentication

## Dependencies

- **.NET 9.0**: Modern .NET framework
- **Microsoft.EntityFrameworkCore**: Entity Framework Core ORM
- **Microsoft.EntityFrameworkCore.Cosmos**: Cosmos DB provider
- **Microsoft.EntityFrameworkCore.SqlServer**: SQL Server provider
- **Microsoft.AspNetCore.Identity**: ASP.NET Core Identity system

## Related Documentation

- [Storage Configuration](../Configuration/Storage-Configuration-Reference.md)
  - [AWS S3 Keys](../Configuration/AWS-S3-AccessKeys.md)
  - [Cloudflare R2 Keys](../Configuration/Cloudflare-R2-AccessKeys.md)
- [Database Configuration](../Configuration/Database-Configuration-Reference.md)

## License

Licensed under the MIT License.

## Contributing

This project is part of the SkyCMS ecosystem. For contribution guidelines, visit the [SkyCMS GitHub repository](https://github.com/CWALabs/SkyCMS).

---

For comprehensive documentation, see the full [Cosmos.Common/README.md](../../Common/README.md)
