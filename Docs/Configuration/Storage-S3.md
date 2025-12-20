---
title: Amazon S3 Storage Configuration
description: Configure Amazon S3 object storage for SkyCMS static assets with access keys and bucket setup
keywords: S3, AWS, storage, configuration, object-storage, static-assets
audience: {developers, administrators}
---

# Amazon S3 with SkyCMS

Amazon S3 is a highly scalable object storage service widely available across AWS regions. SkyCMS integrates seamlessly with S3 for storing and serving static assets.

## Values you need

- **Bucket Name**: S3 bucket name
- **Region**: AWS region where the bucket is located (e.g., `us-east-1`)
- **Access Key ID**: IAM user access key
- **Secret Access Key**: IAM user secret key

## Create an S3 Bucket

1. **AWS Console** → **S3** → **Create bucket**.
2. Fill in:
   - **Bucket name**: Globally unique name (DNS-compliant)
   - **Region**: Choose the region closest to your users
   - **Versioning** (optional): Enable for data recovery
   - **Block public access**: Configure based on your CDN setup
3. Click **Create bucket**.

## Create IAM User with S3 Access (least privilege)

1. **AWS Console** → **IAM** → **Users** → **Create user**.
2. Enable **Access key - Programmatic access**.
3. Attach an inline policy scoped to your bucket:
   ```json
   {
     "Version": "2012-10-17",
     "Statement": {
       {
         "Effect": "Allow",
         "Action": {
           "s3:GetObject",
           "s3:PutObject",
           "s3:DeleteObject",
           "s3:ListBucket"
         },
         "Resource": {
           "arn:aws:s3:::your-bucket-name",
           "arn:aws:s3:::your-bucket-name/*"
         }
       }
     }
   }
   ```
4. Copy the **Access key ID** and **Secret access key** (visible only once).

## Get your bucket information

1. **AWS Console** → **S3** → select your bucket.
2. Note the **Bucket name** and **Region** (shown at the top).

## Configure in SkyCMS

### Using the Setup Wizard (recommended)

1. Deploy SkyCMS with `CosmosAllowSetup=true`.
2. Open the Editor setup wizard.
3. When prompted for **Storage**, paste the connection string:
   ```
   Bucket=your-bucket-name;Region=us-east-1;KeyId=AKIAIOSFODNN7EXAMPLE;Key=wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY;
   ```
4. Enter the **Public URL** for your bucket (e.g., `https://your-bucket-name.s3.us-east-1.amazonaws.com/`).
5. Click **Validate** and proceed.

### Manual Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "StorageConnectionString": "Bucket={your-bucket-name};Region={your region};KeyId={Your KeyID};Key={Your secret};"
  }
}
```

### Environment Variables

```powershell
$env:ConnectionStrings__StorageConnectionString = "Bucket=your-bucket-name;Region=us-east-1;KeyId=AKIAIOSFODNN7EXAMPLE;Key=wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY;"
```

## Static Website Hosting (optional)

1. **AWS Console** → **S3** → **Bucket** → **Properties** → **Static website hosting**.
2. Enable and set **Index document** to `index.html`, **Error document** to `404.html`.
3. Configure a CloudFront distribution for CDN acceleration.

## Best practices

- **Scope IAM credentials** to the specific bucket and actions needed (least privilege).
- **Never commit access keys** to source control; use environment variables or AWS Secrets Manager.
- **Enable versioning** on the bucket for accidental deletion recovery.
- **Enable MFA Delete** (optional) for additional protection against accidental bucket deletion.
- **Rotate access keys** periodically; create new keys before deleting old ones.
- **Monitor access** with S3 access logs and CloudTrail.
- **Use CloudFront** in front of S3 for global CDN acceleration and cache management.

## Tips and troubleshooting

- Connection string format: `Bucket=...;Region=...;KeyId=...;Key=...;`
- Bucket names must be globally unique across all AWS accounts.
- Region format: `us-east-1`, `eu-west-1`, `ap-southeast-1`, etc.
- If validation fails, verify the bucket name, region, and IAM credentials.
- S3 charges per GB stored and per request; monitor usage in AWS Cost Management.
- For better performance, use a region-specific endpoint or CloudFront distribution.

## Further reading

- {AWS S3 Access Keys Guide}(./AWS-S3-AccessKeys.md) — Detailed step-by-step for obtaining credentials
