---
title: Storage Configuration Overview
description: Cloud object storage configuration for static assets including Azure Blob, S3, Cloudflare R2, and Google Cloud
keywords: storage, configuration, Azure-Blob, S3, Cloudflare-R2, Google-Cloud, object-storage
audience: [developers, administrators]
---

# Storage Configuration Overview

SkyCMS stores static web assets (images, CSS/JS, downloads) in cloud object storage. Supported providers:

- Azure Blob Storage
- Amazon S3
- Cloudflare R2 (S3-compatible)
- Google Cloud Storage (S3-compatible)
- Any S3-compatible storage

## How storage configuration works

- You provide a **connection string** that identifies the storage provider and credentials.
- SkyCMS automatically selects the correct storage driver based on the connection string format.
- Connection strings are configured via:
  - **SkyCMS Setup Wizard** (easiest for first-time setup)
  - **appsettings.json** (manual configuration)
  - **Environment variables** (recommended for secrets)

The configuration key is: `ConnectionStrings:StorageConnectionString`

## Quick prerequisites by provider

| Provider | Connection String Format | Where to get values |
| --- | --- | --- |
| **Azure Blob Storage** | `DefaultEndpointsProtocol=...;AccountName=...;AccountKey=...` | Azure Portal → Storage account → Access keys |
| **Amazon S3** | `Bucket=...;Region=...;KeyId=...;Key=...` | AWS Console → IAM → Access keys, S3 bucket info |
| **Cloudflare R2** | `AccountId=...;Bucket=...;KeyId=...;Key=...` | Cloudflare Dashboard → R2 → S3 API Tokens |
| **Google Cloud Storage** | `Bucket=...;Region=...;AccessKey=...;SecretKey=...;ServiceUrl=https://storage.googleapis.com` | Google Cloud Console → Cloud Storage → Interoperability |

## Configure in SkyCMS (common steps)

### Option 1: Setup Wizard (recommended for first-time setup)

1. Deploy SkyCMS with `CosmosAllowSetup=true`.
2. Open the Editor setup wizard.
3. When prompted for **Storage**, paste your connection string and public URL.
4. The wizard validates connectivity and creates containers/buckets.

### Option 2: Manual Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "StorageConnectionString": "your-connection-string-here"
  }
}
```

### Option 3: Environment Variables

Set before starting SkyCMS:
```powershell
$env:ConnectionStrings__StorageConnectionString = "your-connection-string"
```

## Per-provider guides

- [Azure Blob Storage](./Storage-AzureBlob.md)
- [Amazon S3](./Storage-S3.md)
- [Cloudflare R2](./Storage-Cloudflare.md)
- [Google Cloud Storage (S3-compatible)](./Storage-GoogleCloud.md)

## Detailed Configuration

For complete connection string formats, configuration methods, security best practices, and advanced troubleshooting:

- **[Storage Configuration Reference](./Storage-Configuration-Reference.md)** - Detailed connection strings, environment variables, and configuration options

---

## See Also

- **[LEARNING_PATHS: DevOps](../LEARNING_PATHS.md#️-devops--system-administrator)** - Storage setup as part of DevOps journey
- **[Database Configuration](./Database-Overview.md)** - Companion guide for content storage
- **[CDN Configuration](./CDN-Overview.md)** - Performance optimization
- **[Installation & Deployment](../Installation/) - Full deployment guides
- **[Troubleshooting Guide](../Troubleshooting.md)** - General troubleshooting
