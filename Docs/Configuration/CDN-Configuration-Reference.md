---
title: CDN Configuration Reference
description: Complete reference for CDN cache purging configuration with Azure Front Door, CloudFront, Cloudflare, and Sucuri
keywords: CDN, configuration-reference, Azure-Front-Door, CloudFront, Cloudflare, Sucuri
audience: [developers, administrators]
---

# CDN Configuration Reference

Complete reference for CDN cache purging configuration options, credentials, and settings for all supported providers.

SkyCMS integrates with CDN providers to automatically purge caches after publishing, ensuring fresh content appears immediately. Configuration is managed through **Editor → Settings → CDN**.

## Supported CDN Providers

- [Azure Front Door](#azure-front-door)
- [Cloudflare CDN](#cloudflare-cdn)
- [Amazon CloudFront](#amazon-cloudfront)
- [Sucuri CDN/WAF](#sucuri-cdnwaf)

---

## Azure Front Door

### Configuration Values

| Setting | Type | Required | Description |
| --- | --- | --- | --- |
| **Is Azure Front Door** | Boolean | ✅ Yes | Enable if using Front Door (leave unchecked for classic Azure CDN) |
| **Subscription ID** | String (GUID) | ✅ Yes | Azure subscription containing the Front Door profile |
| **Resource Group** | String | ✅ Yes | Resource group name for the Front Door profile |
| **Profile Name** | String | ✅ Yes | Front Door profile name |
| **Endpoint Name** | String | ✅ Yes | Endpoint name inside the profile to purge |

### Required Permissions

- **Recommended role**: CDN Endpoint Contributor (or Contributor on the profile)
- Grants rights to purge endpoint cache
- Can be assigned to service principal or managed identity

### How to Find Values

1. Azure Portal → **Front Door and CDN profiles** → select your profile
2. Copy **Subscription ID** (shown in Essentials)
3. Copy **Resource group** name
4. Copy **Profile name**
5. Go to **Endpoints** tab → copy the **Endpoint name**

### Configuration in SkyCMS

Navigate to **Editor → Settings → CDN → Microsoft** section and enter all required values.

### Example Values

```
Is Azure Front Door: ✓ Enabled
Subscription ID: a1b2c3d4-e5f6-7890-abcd-ef1234567890
Resource Group: myResourceGroup
Profile Name: myFrontDoorProfile
Endpoint Name: myEndpoint
```

---

## Cloudflare CDN

### Configuration Values

| Setting | Type | Required | Description |
| --- | --- | --- | --- |
| **API Token** | String | ✅ Yes | Scoped token with Cache Purge permission |
| **Zone ID** | String | ✅ Yes | ID of the zone to purge |

### Required Permissions

API Token must have:
- **Permission**: Zone → Cache Purge → Purge
- **Zone Resources**: Specific zone (recommended) or All zones

### How to Create API Token

1. Cloudflare Dashboard → **My Profile** → **API Tokens** → **Create Token**
2. Start with **Custom Token**
3. Add permission: **Zone → Cache Purge → Purge**
4. Select zone resources (specific zone recommended)
5. Create token and copy it securely

### How to Find Zone ID

1. Cloudflare Dashboard → select your domain
2. Right sidebar (Overview) → copy **Zone ID**

### Configuration in SkyCMS

Navigate to **Editor → Settings → CDN → Cloudflare** section and enter both values.

### Example Values

```
API Token: a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8
Zone ID: a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6
```

### Security Tips

- Keep token scoped to a single zone for least privilege
- Rotate token periodically and update SkyCMS
- Store in secret manager (Azure Key Vault, AWS Secrets Manager, etc.)

---

## Amazon CloudFront

### Configuration Values

| Setting | Type | Required | Description |
| --- | --- | --- | --- |
| **Distribution ID** | String | ✅ Yes | CloudFront distribution ID (e.g., `E1234567890ABC`) |
| **AWS Access Key ID** | String | ✅ Yes | IAM user access key with invalidation permissions |
| **Secret Access Key** | String | ✅ Yes | IAM user secret key |
| **AWS Region** | String | ✅ Yes | Recommended: `us-east-1` (CloudFront API is global) |

### Required IAM Policy (Least Privilege)

Replace `<DISTRIBUTION_ID>` with your actual distribution ID:

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

### How to Find Values

1. **Distribution ID**:
   - AWS Console → **CloudFront** → **Distributions**
   - Copy the **Distribution ID** for your site

2. **Create IAM User**:
   - AWS Console → **IAM** → **Users** → **Create user**
   - Enable **Access key - Programmatic access**
   - Attach the inline policy above
   - Copy **Access key ID** and **Secret access key** (shown once)

3. **Region**:
   - Use `us-east-1` (recommended for CloudFront API)

### Configuration in SkyCMS

Navigate to **Editor → Settings → CDN → Amazon CloudFront CDN** section and enter all values.

### Example Values

```
Distribution ID: E1234567890ABC
AWS Access Key ID: AKIAIOSFODNN7EXAMPLE
Secret Access Key: wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
AWS Region: us-east-1
```

### Security Tips

- Scope IAM policy to specific distribution ARN
- Rotate IAM keys periodically
- Store credentials in secret manager
- Monitor CloudWatch for invalidation costs

---

## Sucuri CDN/WAF

### Configuration Values

| Setting | Type | Required | Description |
| --- | --- | --- | --- |
| **API Key** | String | ✅ Yes | Sucuri API key for your protected site |
| **API Secret** | String | ✅ Yes | Sucuri API secret |

### How to Find Credentials

1. Log in to Sucuri dashboard
2. Select the site protected with Sucuri
3. Go to **API** (or Account → API)
4. Copy **API Key** and **API Secret** for the site

### Configuration in SkyCMS

Navigate to **Editor → Settings → CDN → Sucuri CDN/Firewall** section and enter both values.

### Example Values

```
API Key: a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6
API Secret: a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0
```

### Security Tips

- Store credentials in secret manager
- Rotate credentials periodically
- Verify site is active in Sucuri before testing

---

## Environment Variables

For automated deployments, CDN configuration can be set via environment variables instead of the Setup Wizard:

### Azure Front Door

```powershell
$env:AzureFrontDoor__IsAzureFrontDoor = "true"
$env:AzureFrontDoor__SubscriptionId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
$env:AzureFrontDoor__ResourceGroup = "myResourceGroup"
$env:AzureFrontDoor__ProfileName = "myFrontDoorProfile"
$env:AzureFrontDoor__EndpointName = "myEndpoint"
```

### Cloudflare

```powershell
$env:Cloudflare__ApiToken = "your-api-token"
$env:Cloudflare__ZoneId = "your-zone-id"
```

### Amazon CloudFront

```powershell
$env:CloudFront__DistributionId = "E1234567890ABC"
$env:CloudFront__AccessKeyId = "AKIAIOSFODNN7EXAMPLE"
$env:CloudFront__SecretAccessKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY"
$env:CloudFront__Region = "us-east-1"
```

### Sucuri

```powershell
$env:Sucuri__ApiKey = "your-api-key"
$env:Sucuri__ApiSecret = "your-api-secret"
```

---

## Testing CDN Configuration

After entering configuration values in **Editor → Settings → CDN**:

1. Click **Save and test settings**
2. SkyCMS will attempt a test invalidation/purge
3. Common failure reasons:
   - Incorrect credentials or IDs
   - Missing permissions on service principal/IAM user
   - Wrong resource names or zone IDs
   - Network connectivity issues

---

## Troubleshooting

### Azure Front Door

**Error**: `AccessDenied` or `Forbidden`
- **Solution**: Verify CDN Endpoint Contributor role is assigned to the identity
- Check subscription ID, resource group, profile name, and endpoint name are correct

**Error**: `ResourceNotFound`
- **Solution**: Verify profile name and endpoint name match exactly (case-sensitive)

### Cloudflare

**Error**: `Invalid API token`
- **Solution**: Regenerate token with Zone → Cache Purge → Purge permission
- Verify zone ID matches the domain being purged

**Error**: `Zone not found`
- **Solution**: Double-check zone ID from Cloudflare dashboard

### CloudFront

**Error**: `AccessDenied`
- **Solution**: Verify IAM policy includes both `CreateInvalidation` and `GetInvalidation`
- Check distribution ARN matches policy resource

**Error**: `NoSuchDistribution`
- **Solution**: Verify distribution ID is correct (check CloudFront console)

### Sucuri

**Error**: `Authentication failed`
- **Solution**: Verify API key and secret are correct
- Ensure site is active in Sucuri dashboard

---

## Security Best Practices

1. **Use Least Privilege**: Grant only the minimum permissions needed
2. **Rotate Credentials**: Regularly rotate API keys, tokens, and secrets
3. **Use Secret Managers**: Store credentials in Azure Key Vault, AWS Secrets Manager, etc.
4. **Monitor Access**: Enable logging and monitoring for CDN operations
5. **Scope Narrowly**: Limit credentials to specific resources (distributions, zones, endpoints)

---

## See Also

- **[CDN Overview](./CDN-Overview.md)** - Getting started with CDN integration
- **[Azure Front Door CDN](./CDN-AzureFrontDoor.md)** - Detailed Azure Front Door guide
- **[Cloudflare CDN](./CDN-Cloudflare.md)** - Detailed Cloudflare guide
- **[Amazon CloudFront CDN](./CDN-CloudFront.md)** - Detailed CloudFront guide
- **[Sucuri CDN/WAF](./CDN-Sucuri.md)** - Detailed Sucuri guide
- **[Configuration Overview](./README.md)** - Index of all configuration documentation
- **[Publishing Overview](../Publishing-Overview.md)** - Publishing workflow with CDN cache purging
- **[Troubleshooting Guide](../Troubleshooting.md)** - General troubleshooting
- **[Main Documentation Hub](../index.md)** - Browse all documentation
