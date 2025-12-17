---
title: Amazon CloudFront CDN Configuration
description: Configure CloudFront cache purging with distribution ID and IAM credentials
keywords: CloudFront, CDN, AWS, cache-purging, invalidation, configuration
audience: [developers, devops]
---

# Configure Amazon CloudFront CDN in SkyCMS

SkyCMS can purge CloudFront caches after publishes. Use the CDN settings screen to store a CloudFront distribution ID and an IAM key pair that can create invalidations.

## Prerequisites

- An existing CloudFront distribution pointing at your origin (S3, custom, or other).
- An AWS IAM user (programmatic access) with permission to create and read invalidations for that distribution.
- Your distribution ID (e.g., `E1234567890ABC`).

## Configure CloudFront in SkyCMS

1. In the Editor, go to **Settings → CDN**.
2. Under **Amazon CloudFront CDN**, enter:
   - **Distribution ID**: Your CloudFront distribution ID.
   - **AWS Access Key ID / Secret Access Key**: IAM credentials scoped to CloudFront invalidations.
   - **AWS Region**: Recommended `us-east-1` (CloudFront is global; us-east-1 is used for the API). Other regions work for IAM, but keep it consistent with your AWS account's default.
3. Click **Save and test settings**. SkyCMS will attempt a test invalidation; failures usually indicate missing permissions or an incorrect Distribution ID.

## Minimum IAM policy (least privilege)

Scope this policy to the specific distribution ARN. Replace `<DISTRIBUTION_ID>` with your ID.

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "cloudfront:CreateInvalidation",
        "cloudfront:GetInvalidation"
      ],
      "Resource": "arn:aws:cloudfront::*:distribution/<DISTRIBUTION_ID>"
    }
  ]
}
```

Attach this policy to the IAM user whose keys you enter in SkyCMS.

## Get the required values in the AWS Console

1. **Find the distribution ID**
   - AWS Console → **CloudFront** → **Distributions** → copy the **Distribution ID** for the site you want to purge.

2. **Create an IAM user for invalidations**
   - AWS Console → **IAM** → **Users** → **Create user** → enable **Access key - Programmatic access**.
   - Attach the inline policy above (or an equivalent policy) to the user. Keep it scoped to the target distribution.
   - After creation, copy the **Access key ID** and **Secret access key** once. Store them securely (Secrets Manager, Key Vault, etc.).

3. **Region to use in SkyCMS**
   - Use `us-east-1` for CloudFront API calls (recommended). The API is global, but the SDK still expects a region.

4. **Test**
   - In SkyCMS, click **Save and test settings**. If the test fails, verify the Distribution ID and the IAM policy permissions.

## Tips and troubleshooting

- If invalidations fail with `AccessDenied`, re-check the distribution ARN in the policy and ensure both `CreateInvalidation` and `GetInvalidation` are allowed.
- Large invalidations can take time; SkyCMS sends targeted path invalidations to keep them fast.
- Rotate IAM keys periodically and update the values in SkyCMS.
