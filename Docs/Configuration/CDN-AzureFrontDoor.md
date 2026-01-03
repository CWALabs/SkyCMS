---
title: Azure Front Door CDN Configuration
description: Configure Azure Front Door or Azure CDN cache purging
keywords: Azure-Front-Door, Azure-CDN, cache-purging, configuration
audience: [developers, devops]
version: 2.0
last_updated: "2026-01-03"
stage: stable
read_time: 4
---

# Azure Front Door CDN with SkyCMS

SkyCMS can purge Azure Front Door (Standard/Premium) or Azure CDN endpoints after publishing.

## When to use this
- You front your site with Azure Front Door/modern Azure CDN and need automated cache purge.

## Why this matters
- Stale cache causes outdated content; correct role/IDs are required for purge success.

## Key takeaways
- Collect Subscription ID, Resource Group, Profile Name, Endpoint Name; role: CDN Endpoint Contributor (or equivalent).
- Configure in Editor → Settings → CDN (Microsoft) and test before production.
- Managed identity/service principal must have role on the profile scope.

## Prerequisites
- Front Door/modern CDN profile + endpoint; Azure identity with purge permissions.

## Quick path
1. Copy Sub ID, RG, Profile, Endpoint from portal.
2. Assign CDN Endpoint Contributor to your identity/service principal on the profile.
3. Enter values in Settings → CDN; save and test; then publish and verify.

## Values you need

- **Subscription ID**: Azure subscription that contains the Front Door profile
- **Resource Group**: Resource group name for the Front Door profile
- **Profile Name**: Front Door profile name
- **Endpoint Name**: Endpoint name inside the profile

## Required permissions

Use a service principal or managed identity with rights on the Front Door profile:
- Recommended role: **CDN Endpoint Contributor** (or **Contributor** on the profile) to allow cache purge.

## Get the values in Azure Portal

1. Portal → **Front Door and CDN profiles** → select your profile.
2. Copy **Subscription ID** (shown in Essentials).
3. Copy **Resource group** name.
4. Copy **Profile name** (the profile you opened).
5. Go to **Endpoints** tab → copy the **Endpoint name** you want SkyCMS to purge.

## Configure in SkyCMS

1. In the Editor, go to **Settings → CDN**.
2. Under **Microsoft**, enter:
   - **Is Azure Front Door**: enable if you are using Front Door (leave unchecked for classic Azure CDN).
   - **Subscription ID**: paste the ID.
   - **Resource Group**: the resource group containing the profile.
   - **Profile Name**: the Front Door profile name.
   - **Endpoint Name**: the endpoint to purge.
3. Save and test. If the test fails, verify the role assignment on the identity and the endpoint name.

## Tips

- If using a service principal, ensure the app registration has the CDN role on the profile scope.
- If using managed identity in Azure, assign the role to the identity on the Front Door profile.
- Purges are path-based; keep paths small to speed up invalidations.
