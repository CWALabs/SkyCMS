---
title: CDN Integration Overview
description: CDN cache purging configuration for Azure Front Door, CloudFront, Cloudflare, and Sucuri
keywords: CDN, cache-purging, Azure-Front-Door, CloudFront, Cloudflare, Sucuri
audience: [developers, devops]
---

# CDN Integration Overview

SkyCMS can purge CDN caches after publish so fresh content appears immediately. Supported providers:

- Azure Front Door
- Cloudflare CDN
- Amazon CloudFront
- Sucuri CDN/WAF

## How CDN integration works

- You provide provider-specific credentials/IDs in **Editor → Settings → CDN**.
- SkyCMS issues targeted cache invalidations (paths) after publishes.
- Each provider requires narrowly scoped credentials (least privilege) to create/read invalidations or purge cache.

## Quick prerequisites by provider

| Provider | Values you need | Permission scope |
| --- | --- | --- |
| Azure Front Door | Subscription ID, Resource Group, Front Door Profile Name, Endpoint Name | Azure role with rights to purge endpoint cache (e.g., CDN Endpoint Contributor on the profile) |
| Cloudflare | Zone ID, API Token with Zone.Cache Purge | API Token scoped to the zone, permission: **Zone.Cache Purge** |
| CloudFront | Distribution ID, IAM Access Key/Secret, AWS Region (use `us-east-1`) | IAM policy allowing `cloudfront:CreateInvalidation` + `cloudfront:GetInvalidation` on the distribution ARN |
| Sucuri | API Key, API Secret | Dashboard API key/secret for the protected site |

## Configure in SkyCMS (common steps)

1. In the Editor, open **Settings → CDN**.
2. Fill in the fields for your provider (see per-provider guides below).
3. Click **Save and test settings**. Fix any permission or ID errors before going live.

## Per-provider guides

- [Azure Front Door CDN](./CDN-AzureFrontDoor.md)
- [Cloudflare CDN](./CDN-Cloudflare.md)
- [Amazon CloudFront CDN](./CDN-CloudFront.md)
- [Sucuri CDN/WAF](./CDN-Sucuri.md)

---

## See Also

- **[Database Configuration](./Database-Overview.md)** - Companion configuration guide
- **[Storage Configuration](./Storage-Overview.md)** - Companion configuration guide
- **[Configuration Overview](./README.md)** - Index of all configuration documentation
- **[LEARNING_PATHS: DevOps](../LEARNING_PATHS.md#️-devops--system-administrator)** - CDN setup for DevOps professionals
- **[Publishing Overview](../Publishing-Overview.md)** - Publishing workflow with CDN cache purging
- **[Troubleshooting Guide](../Troubleshooting.md)** - CDN troubleshooting
- **[Main Documentation Hub](../README.md)** - Browse all documentation
