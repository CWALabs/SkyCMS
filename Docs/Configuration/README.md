---
title: Configuration Documentation
description: Quick reference index for all SkyCMS configuration options including multi-tenant, database, storage, and CDN
keywords: configuration, settings, multi-tenant, database, storage, CDN, email
audience: [developers, administrators]
---

# Configuration Documentation

Quick reference index for all SkyCMS configuration documentation.

---

## Quick Links

- **[Multi-Tenant Configuration](./Multi-Tenant-Configuration.md)** ⭐ - Set up multiple independent sites on shared infrastructure
- **[Database Configuration Overview](./Database-Overview.md)** - Supported providers and setup steps
- **[Storage Configuration Overview](./Storage-Overview.md)** - File storage providers and setup
- **[Email Configuration Overview](./Email-Overview.md)** - Transactional email providers and setup
- **[CDN Configuration Overview](./CDN-Overview.md)** - Content delivery and performance optimization

---

## By Topic

### Database Configuration

| Topic | Link |
|-------|------|
| Overview & Comparison | [Database-Overview.md](./Database-Overview.md) |
| Configuration Reference | [Database-Configuration-Reference.md](./Database-Configuration-Reference.md) |
| **Providers:** | |
| Azure Cosmos DB | [Database-CosmosDB.md](./Database-CosmosDB.md) |
| SQL Server / Azure SQL | [Database-SQLServer.md](./Database-SQLServer.md) |
| MySQL | [Database-MySQL.md](./Database-MySQL.md) |
| SQLite | [Database-SQLite.md](./Database-SQLite.md) |

### Storage Configuration

| Topic | Link |
|-------|------|
| Overview & Comparison | [Storage-Overview.md](./Storage-Overview.md) |
| Configuration Reference | [Storage-Configuration-Reference.md](./Storage-Configuration-Reference.md) |
| **Providers:** | |
| Azure Blob Storage | [Storage-AzureBlob.md](./Storage-AzureBlob.md) |
| Amazon S3 | [Storage-S3.md](./Storage-S3.md) |
| Cloudflare R2 | [Storage-Cloudflare.md](./Storage-Cloudflare.md) |
| Google Cloud Storage | [Storage-GoogleCloud.md](./Storage-GoogleCloud.md) |

### CDN Configuration

| Topic | Link |
|-------|------|
| Overview & Comparison | [CDN-Overview.md](./CDN-Overview.md) |
| **Providers:** | |
| Azure Front Door | [CDN-AzureFrontDoor.md](./CDN-AzureFrontDoor.md) |
| Cloudflare | [CDN-Cloudflare.md](./CDN-Cloudflare.md) |
| Amazon CloudFront | [CDN-CloudFront.md](./CDN-CloudFront.md) |
| Sucuri | [CDN-Sucuri.md](./CDN-Sucuri.md) |

### Email Configuration

| Topic | Link |
|-------|------|
| Overview & Comparison | [Email-Overview.md](./Email-Overview.md) |
| Configuration Reference | [Email-Configuration-Reference.md](./Email-Configuration-Reference.md) |
| **Providers:** | |
| SendGrid | [Email-SendGrid.md](./Email-SendGrid.md) |
| Azure Communication Services | [Email-AzureCommunicationServices.md](./Email-AzureCommunicationServices.md) |
| SMTP | [Email-SMTP.md](./Email-SMTP.md) |
| No-Op (Development) | [Email-None.md](./Email-None.md) |

---

## Documentation Structure

Each configuration topic follows this pattern:

1. **Overview** - Comparison of providers, when to use each
2. **Configuration Reference** - Connection string formats, environment variables, setup
3. **Provider-Specific Guides** - Detailed setup for each provider

This structure helps you:
- **Quick Comparison** - See all options in overview
- **General Setup** - Configuration format in reference
- **Detailed Setup** - Step-by-step for your chosen provider

---

## Finding Information

### I want to...

- **Choose a database provider** → See [Database-Overview.md](./Database-Overview.md)
- **Set up database in SkyCMS** → See [Database-Configuration-Reference.md](./Database-Configuration-Reference.md)
- **Configure Cosmos DB** → See [Database-CosmosDB.md](./Database-CosmosDB.md)
- **Change storage provider** → See [Storage-Overview.md](./Storage-Overview.md)
- **Set up file storage** → See [Storage-Configuration-Reference.md](./Storage-Configuration-Reference.md)
- **Use Azure Blob Storage** → See [Storage-AzureBlob.md](./Storage-AzureBlob.md)
- **Configure email sending** → See [Email-Overview.md](./Email-Overview.md)
- **Set up SMTP email** → See [Email-SMTP.md](./Email-SMTP.md)
- **Use SendGrid for email** → See [Email-SendGrid.md](./Email-SendGrid.md)
- **Speed up my site with CDN** → See [CDN-Overview.md](./CDN-Overview.md)
- **Configure Cloudflare** → See [CDN-Cloudflare.md](./CDN-Cloudflare.md)

---

## File Naming Convention

All configuration files follow **PascalCase-with-hyphens** format:

```
✅ CORRECT:
Database-Overview.md
Database-Configuration-Reference.md
Database-CosmosDB.md
Storage-AzureBlob.md
CDN-Cloudflare.md
```

---

**Last Updated:** December 17, 2025

---

## Related Documentation

- **[Authentication Overview](../Authentication-Overview.md)** - User authentication and authorization
- **[Publishing Overview](../Publishing-Overview.md)** - Publishing modes and workflows
- **[Widgets Overview](../Widgets-Overview.md)** - Reusable UI components
- **[Main Documentation Hub](../README.md)** - Browse all documentation
- **[LEARNING_PATHS](../LEARNING_PATHS.md)** - Role-based documentation journeys
