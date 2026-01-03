---
title: Sucuri CDN/WAF Configuration
description: Configure Sucuri CDN and WAF cache purging
keywords: Sucuri, CDN, WAF, cache-purging, security, configuration
audience: [developers, devops]
version: 2.0
last_updated: "2026-01-03"
stage: stable
read_time: 3
---

# Sucuri CDN/WAF with SkyCMS

SkyCMS can trigger cache purge on Sucuri after publish.

## When to use this
- You protect your site with Sucuri CDN/WAF and need automated cache purge.

## Why this matters
- Correct key/secret are required; stale cache delays content updates.

## Key takeaways
- Use site-specific API key/secret; store in secret manager.
- Configure in Editor → Settings → CDN (Sucuri) and test.
- Rotate credentials periodically.

## Prerequisites
- Active Sucuri-protected site and its API key/secret.

## Quick path
1. Copy Sucuri API key/secret from dashboard.
2. Enter in Settings → CDN (Sucuri); save and test.
3. Publish and verify purge succeeds.

## Values you need

- **API Key**
- **API Secret**

## Get credentials from Sucuri dashboard

1. Log in to your Sucuri dashboard.
2. Select the site you protect with Sucuri.
3. Go to **API** (or Account → API). Copy the **API Key** and **API Secret** for that site.

## Configure in SkyCMS

1. In the Editor, go to **Settings → CDN**.
2. Under **Sucuri CDN/Firewall**, enter:
   - **API Key**
   - **API Secret**
3. Save and test. If the test fails, verify the key/secret and that your site is active in Sucuri.

## Tips

- Store the key/secret in your secret manager (Key Vault, Secrets Manager, etc.) and inject via environment variables.
- Rotate the credentials periodically and update SkyCMS.
