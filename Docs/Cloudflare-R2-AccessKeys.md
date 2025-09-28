# Cloudflare R2 access keys for SkyCMS (quick guide)

Cloudflare R2 is S3‑compatible. To connect programmatically you’ll typically need your Account ID, the R2 bucket name, and an S3 API token consisting of an Access Key ID and Secret Access Key.

Note: The current SkyCMS release doesn’t expose a custom S3 endpoint in configuration, which R2 requires (e.g., `https://<account-id>.r2.cloudflarestorage.com`). You can still prepare your credentials with this guide. For a turnkey setup today, use Azure Blob Storage or Amazon S3; R2 support requires a small code change to allow a custom endpoint.

## 1) Find your Cloudflare Account ID

Console path: Cloudflare Dashboard → select your account → R2 → S3 API Tokens

- In the S3 API section you’ll see your Account ID and the S3 endpoint format. Copy the Account ID.
- You can also find the Account ID on the Account Home/Overview page for your Cloudflare account.

## 2) Get or create your R2 bucket name

Console path: Cloudflare Dashboard → R2 → Buckets

- If you already have a bucket, copy its name from the list (e.g., `site-content`).
- To create a new one, click "Create bucket", enter a unique bucket name, and finish setup. Copy the bucket name.

## 3) Generate an S3 API Token (Access Key ID and Secret Access Key)

Console path: Cloudflare Dashboard → R2 → S3 API Tokens → Create API token

1. Click "Create API token" (S3).
1. Give it a name (e.g., `skycms-r2-bucket`).
1. Permissions:
   - Choose a permission that allows read, write, and delete, such as "Object Read & Write & Delete" (or the equivalent "Edit" for R2 Storage).
1. Bucket scope:
   - Choose "Specific buckets" and select your target bucket to keep the token least‑privileged.
1. Create the token and copy the Access Key ID and Secret Access Key. Store them securely (you won’t be able to view the secret again).

## 4) Endpoint information (for S3‑compatible clients)

R2 uses a custom S3 endpoint:

- Endpoint format: `https://<account-id>.r2.cloudflarestorage.com`
- Example: if your Account ID is `1234567890abcdef`, your endpoint is `https://1234567890abcdef.r2.cloudflarestorage.com`

Some S3 clients also require a region. For R2 this is often set to `auto` or a placeholder region like `us-east-1`, depending on the SDK. Consult your SDK’s R2 guidance.

## 5) Configure SkyCMS (when custom endpoint is supported)

When SkyCMS exposes a custom S3 endpoint setting, you’ll use a connection string similar to:

```json
{
   "ConnectionStrings": {
      "StorageConnectionString": "AccountId={Account ID};Bucket={bucket name};KeyId={access-key-id};Key={secret-access-key};"
   }
}
```

- Replace `{bucket}`, `{access-key-id}`, `{secret-access-key}`, and `{account-id}` with your values.

## Troubleshooting

- Access denied: ensure the S3 API token permission includes read, write, and delete and is scoped to the correct bucket.
- Wrong bucket: verify the bucket name exactly matches the one in R2 → Buckets.
- Endpoint errors: confirm you’re using the account‑scoped R2 endpoint and that your client/SDK supports custom S3 endpoints.
- Secret visibility: you can’t view a token’s secret again; create a new token if it’s lost and delete the old one.
