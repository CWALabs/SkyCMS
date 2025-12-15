# Google Cloud Storage (S3-compatible) with SkyCMS

Google Cloud Storage (GCS) is a scalable, durable object storage service. SkyCMS can integrate via GCS's S3-compatible API.

## Values you need

- **Bucket Name**: GCS bucket name
- **Region**: GCS region (e.g., `us-central1`)
- **Access Key ID**: S3 interoperability access key
- **Secret Access Key**: S3 interoperability secret key
- **Service URL**: `https://storage.googleapis.com`

## Create a Google Cloud Storage Bucket

1. **Google Cloud Console** → **Cloud Storage** → **Buckets** → **Create**.
2. Fill in:
   - **Bucket name**: Globally unique name
   - **Location**: Choose a region or multi-region
   - **Storage class**: Standard (most common)
   - **Access control**: Uniform (recommended)
3. Click **Create**.

## Enable S3 Interoperability

1. **Google Cloud Console** → **Cloud Storage** → **Settings** → **Interoperability**.
2. Click **Create a service account key** (or use existing key).
3. Note the **Access Key** and **Secret** (these are the S3 credentials).
4. Alternatively, create a service account with S3 interoperability:
   - Go to **IAM & Admin** → **Service Accounts** → **Create Service Account**.
   - Assign **Storage Object Admin** role.
   - Create a key (JSON format) and extract `access_key` and `secret_key`.

## Set Bucket Permissions

Grant the service account access to your bucket:

```bash
gsutil iam ch serviceAccount:YOUR_SERVICE_ACCOUNT@YOUR_PROJECT.iam.gserviceaccount.com:objectAdmin gs://your-bucket-name
```

Or in the console:
1. **Cloud Storage** → **Bucket** → **Permissions** (Share bucket) → **Add principal**.
2. Add the service account with **Storage Object Admin** role.

## Configure in SkyCMS

### Using the Setup Wizard (recommended)

1. Deploy SkyCMS with `CosmosAllowSetup=true`.
2. Open the Editor setup wizard.
3. When prompted for **Storage**, paste the connection string:
   ```
   Bucket=your-bucket-name;Region=us-central1;AccessKey=YOUR_ACCESS_KEY;SecretKey=YOUR_SECRET_KEY;ServiceUrl=https://storage.googleapis.com
   ```
4. Enter the **Public URL**:
   ```
   https://storage.googleapis.com/your-bucket-name/
   ```
5. Click **Validate** and proceed.

### Manual Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "StorageConnectionString": "Bucket=your-bucket-name;Region=us-central1;AccessKey=YOUR_ACCESS_KEY;SecretKey=YOUR_SECRET_KEY;ServiceUrl=https://storage.googleapis.com"
  }
}
```

### Environment Variables

```powershell
$env:ConnectionStrings__StorageConnectionString = "Bucket=your-bucket-name;Region=us-central1;AccessKey=YOUR_ACCESS_KEY;SecretKey=YOUR_SECRET_KEY;ServiceUrl=https://storage.googleapis.com"
```

## Enable Public Access (optional)

If SkyCMS publishes to a publicly-readable bucket:

1. **Cloud Storage** → **Bucket** → **Permissions**.
2. Add a principal: `allUsers` with **Storage Object Viewer** role.

For better control, use a Cloud CDN or load balancer in front of GCS.

## Best practices

- **Use service accounts** instead of user credentials for application access.
- **Scope permissions** to specific buckets and roles (least privilege).
- **Never commit access keys** to source control; use environment variables or Secret Manager.
- **Rotate keys** periodically; create new keys before deleting old ones.
- **Enable versioning** on the bucket for accidental deletion recovery.
- **Use Cloud CDN** in front of GCS for global performance and caching.
- **Monitor costs** in Cloud Cost Management (GCS charges per operation and egress).

## Tips and troubleshooting

- Connection string format: `Bucket=...;Region=...;AccessKey=...;SecretKey=...;ServiceUrl=https://storage.googleapis.com`
- **Must include `ServiceUrl=https://storage.googleapis.com`** for S3 interoperability routing.
- Region format: `us-central1`, `europe-west1`, `asia-northeast1`, etc.
- Bucket names must be globally unique across all GCS projects.
- If validation fails, verify the bucket name, region, and S3 credentials.
- GCS S3 interoperability has some limitations; for advanced GCS features, use the native GCS API.

## Further reading

- [Google Cloud Storage Documentation](https://cloud.google.com/storage/docs)
- [S3 Interoperability](https://cloud.google.com/storage/docs/interoperability)
