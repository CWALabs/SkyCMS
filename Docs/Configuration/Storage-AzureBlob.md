---
title: Azure Blob Storage Configuration
description: Configure Azure Blob Storage for SkyCMS static assets with connection strings and container setup
keywords: Azure-Blob, storage, configuration, Azure, object-storage, static-assets
audience: [developers, administrators]
---

# Azure Blob Storage with SkyCMS

Azure Blob Storage is a fully managed, scalable cloud object storage service. SkyCMS integrates seamlessly for storing and serving static assets.

## Values you need

- **Account Name**: Storage account name
- **Account Key**: Primary or Secondary key for authentication
- **Container**: Container name for your assets (optional; SkyCMS can create it)

## Create an Azure Storage Account

1. **Azure Portal** → **Create a resource** → search **"Storage account"** → **Create**.
2. Fill in:
   - **Storage account name**: Globally unique name (lowercase, alphanumeric)
   - **Subscription**: Choose your subscription
   - **Resource Group**: Create new or select existing
   - **Location**: Choose a region
   - **Performance**: Standard (sufficient for most use cases)
   - **Replication**: Locally-redundant storage (LRS) or geo-redundant (GRS) based on needs
3. Click **Create** and wait for deployment.

## Get your credentials

1. Portal → **Storage accounts** → select your account.
2. Click **Access keys** (under Security + networking).
3. Copy:
   - **Storage account name**: Your account name
   - **Key**: Primary Key (or Secondary Key)
   - **Connection string**: The full connection string (easier to copy)

## Configure in SkyCMS

### Using the Setup Wizard (recommended)

1. Deploy SkyCMS with `CosmosAllowSetup=true`.
2. Open the Editor setup wizard.
3. When prompted for **Storage**, paste the connection string:
   ```
   DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=your-key;EndpointSuffix=core.windows.net
   ```
4. Enter the **Public URL** for your storage (e.g., `https://myaccount.blob.core.windows.net/`).
5. Click **Validate** and proceed.

### Manual Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=your-key;EndpointSuffix=core.windows.net"
  }
}
```

### Using Managed Identity (recommended for production)

Instead of storing keys in config, use managed identity:

```json
{
  "ConnectionStrings": {
    "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=AccessToken;EndpointSuffix=core.windows.net"
  }
}
```

Then assign the **Storage Blob Data Contributor** role to your app's identity on the storage account.

### Environment Variables

```powershell
$env:ConnectionStrings__StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=your-key;EndpointSuffix=core.windows.net"
```

## Static Website Hosting (optional)

SkyCMS can serve published content directly from Azure static website hosting:

1. Portal → **Storage account** → **Data management** → **Static website**.
2. Enable and set **Index document** to `index.html`, **Error document** to `404.html`.
3. SkyCMS automatically publishes to this container.

## Best practices

- **Use managed identity** in production (instead of keys). Simplifies credential management and improves security.
- **Secure your keys** in Azure Key Vault, not in code.
- **Use geo-redundant replication (GRS)** for critical data; LRS is sufficient for cached assets.
- **Enable public access** only to the specific containers SkyCMS needs.
- **Rotate keys** periodically.
- **Enable blob versioning** for accidental deletion recovery.

## Tips and troubleshooting

- Connection string format: `DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net`
- If validation fails, verify the account name, key, and endpoint suffix.
- Azure Blob Storage charges per GB stored and per operation; monitor usage in the Azure Portal.
- For CDN acceleration, integrate with Azure CDN or Front Door.
