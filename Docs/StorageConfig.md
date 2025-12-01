# Storage Configuration

SkyCMS stores static web assets (images, CSS/JS, downloads) in cloud object storage. It automatically selects a storage driver based on the connection string configured under:

`ConnectionStrings:StorageConnectionString`

If that key is not present, the system also checks:

`ConnectionStrings:AzureBlobStorageConnectionString`

Use the following structure in `appsettings.json` (or environment variables/secrets):

```json
{
   "ConnectionStrings": {
      "StorageConnectionString": "<your-storage-connection-string>"
   }
}
```

Jump to:

- [Storage Configuration](#storage-configuration)
  - [Azure Blob Storage](#azure-blob-storage)
  - [Amazon S3](#amazon-s3)
  - [Cloudflare R2 (S3-compatible)](#cloudflare-r2-s3-compatible)
  - [Which storage should I use?](#which-storage-should-i-use)
  - [Static website hosting (Azure)](#static-website-hosting-azure)
  - [Security and secrets](#security-and-secrets)
  - [Troubleshooting](#troubleshooting)

---

## Azure Blob Storage

Azure storage connection string:

```json
{
   "ConnectionStrings": {
      "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName={account};AccountKey={key};EndpointSuffix=core.windows.net"
   }
}
```

Find values in the Azure Portal:

1. Open the [Azure Portal](https://portal.azure.com) → Storage accounts → select your account
2. Security + networking → Access keys → copy a connection string (Key1 or Key2)

Managed identity (no secret in config):

```json
{
   "ConnectionStrings": {
      "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName={account};AccountKey=AccessToken;EndpointSuffix=core.windows.net"
   }
}
```

> Note: “AccountKey=AccessToken” enables Azure Default Credential in code. Ensure your app’s identity has Blob Data access roles on the storage account.

---

## Amazon S3

Quick setup guide: see [AWS S3 access keys for SkyCMS](./AWS-S3-AccessKeys.md) for a step‑by‑step, least‑privilege walkthrough (create IAM user, bucket‑scoped policy, and access keys).

```json
{
   "ConnectionStrings": {
      "StorageConnectionString": "Bucket={bucket};Region={aws-region};KeyId={access-key-id};Key={secret-access-key};"
   }
}
```

Where to find values in AWS Console:

1. S3 → choose your bucket → note the bucket name and region (e.g., `us-west-2`)
2. IAM → Users → your user → Security credentials → Create access key → copy Access key ID and Secret access key

Best practice: Scope IAM permissions to the specific bucket and required actions (GetObject, PutObject, ListBucket, DeleteObject).

---

## Cloudflare R2 (S3-compatible)

Cloudflare R2 is S3-compatible. With SkyCMS you’ll provide your Account ID, bucket name, and S3-style credentials (Key ID/Secret).

Quick setup guide: see [Cloudflare R2 access keys for SkyCMS](./Cloudflare-R2-AccessKeys.md) to find your Account ID and bucket, and to generate an S3 API token (read/write/delete). For edge/origin-less hosting with Cloudflare, see [Cloudflare Edge Hosting](./CloudflareEdgeHosting.md) for instructions on binding your R2 bucket and configuring Cloudflare Rules (no Worker required).

Format the connection string for R2 storage in the following manner. Note it requires
an Account ID, Bucket name, Key ID and Key Secret:

```json
{
   "ConnectionStrings": {
      "StorageConnectionString": "AccountId={Account ID};Bucket={bucket name};KeyId={access-key-id};Key={secret-access-key};"
   }
}
```

---

## Which storage should I use?

Use what your team already knows when possible. Quick guidance:

| Provider           | Best for                                      | Pros                                                | Considerations |
|--------------------|-----------------------------------------------|-----------------------------------------------------|----------------|
| Azure Blob Storage | Azure-native deployments, static website CDN  | First-class Azure integration, managed identity     | Requires Azure account/roles |
| Amazon S3          | AWS-native or multi-cloud compatibility       | Ubiquitous, scalable, rich tooling                  | Access keys management, region selection |
| Cloudflare R2      | S3-compatible, egress-friendly pricing        | Cost model benefits                                 | Custom endpoint may be required for R2; see Cloudflare docs and [CloudflareEdgeHosting.md](./CloudflareEdgeHosting.md) |

---

## Static website hosting (Azure)

SkyCMS can programmatically enable Azure Storage static website hosting. This requires a standard key-based connection string.

> Important: When using managed identity (`AccountKey=AccessToken`), the code cannot enable static website due to SDK restrictions. Use a key-based connection temporarily to enable it, or enable it manually in the portal.

---

## Security and secrets

- Do not commit secrets to source control.
- Prefer environment variables, ASP.NET Core User Secrets (for local dev), or Azure Key Vault.
- Enforce least-privilege on credentials; rotate regularly.

---

## Troubleshooting

- Provider detection is based on the connection string:
   - Starts with `DefaultEndpointsProtocol=` → Azure Blob Storage
   - Contains `Bucket=` → Amazon S3
- Ensure the connection key is `ConnectionStrings:StorageConnectionString` (or `AzureBlobStorageConnectionString` as fallback).
- For Azure managed identity, grant the app identity "Storage Blob Data Contributor" (or finer-grained roles) on the target account.
- For S3, verify region matches the bucket's region and keys are valid.

---

## See Also

- **[Database Configuration](./DatabaseConfig.md)** - Configure your database provider
- **[AWS S3 Access Keys](./AWS-S3-AccessKeys.md)** - Step-by-step S3 setup guide
- **[Cloudflare R2 Access Keys](./Cloudflare-R2-AccessKeys.md)** - Step-by-step R2 setup guide
- **[Cloudflare Edge Hosting](./CloudflareEdgeHosting.md)** - Origin-less hosting with R2
- **[Azure Installation](./AzureInstall.md)** - Deploy SkyCMS to Azure
- **[Main Documentation Hub](./README.md)** - Browse all documentation
