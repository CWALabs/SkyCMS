---
title: Cloudflare CDN Configuration
description: Configure Cloudflare cache purging with API tokens
keywords: Cloudflare, CDN, cache-purging, API, configuration
audience: [developers, devops]
version: 2.0
last_updated: "2026-01-03"
stage: stable
read_time: 4
---

# Cloudflare CDN with SkyCMS

SkyCMS can purge Cloudflare cache after publish.

## When to use this
- You use Cloudflare CDN and need automated cache purge after publishes.

## Why this matters
- Stale cache hides updates; correct token scope is required for purge success.

## Key takeaways
- Use scoped API token with Zone.Cache Purge on the target zone; keep to single zone.
- Configure in Editor → Settings → CDN with API token + Zone ID; test before go-live.
- Rotate tokens periodically and store in a secret manager.

## Prerequisites
- Cloudflare zone, Zone ID, and scoped API token with purge permission.

## Quick path
1. Create scoped token (Zone → Cache Purge → Purge) for the target zone; copy Zone ID.
2. Enter token + Zone ID in Settings → CDN (Cloudflare); save and test.
3. Publish and confirm invalidations complete.

## Values you need

- **Zone ID**: ID of the zone you want to purge.
- **API Token**: Scoped token with permission to purge cache for that zone.

## Create a scoped API token

1. Cloudflare Dashboard → **My Profile** → **API Tokens** → **Create Token**.
2. Start with **Custom Token**.
3. Permissions: add **Zone → Cache Purge → Purge**.
4. Zone Resources: choose the specific zone (recommended) or All zones (broader scope).
5. Create token; copy and store it securely.

## Find the Zone ID

1. Cloudflare Dashboard → choose your domain.
2. On the right sidebar (Overview), copy the **Zone ID**.

## Configure in SkyCMS

1. In the Editor, go to **Settings → CDN**.
2. Under **Cloudflare**, enter:
   - **API Token**: the scoped token you created.
   - **Zone ID**: the zone you want to purge.
3. Save and test. If the test fails, re-check token permissions and Zone ID.

## Tips

- Keep the token scoped to a single zone for least privilege.
- Rotate the token periodically and update SkyCMS.
