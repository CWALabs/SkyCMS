# Cloudflare Edge Hosting: Origin-less Static Website Architecture

This guide shows how to host a static site on Cloudflare using an origin-less (edge) pattern with R2 object storage and Cloudflare Rules for request handling at the edge. It also explains how to configure SkyCMS to deploy your build output to R2.

Key idea: Unlike traditional static hosting that serves from a single origin, edge/origin-less sites are distributed and executed at Cloudflare's global edge—improving latency, resilience, and cost profiles.

## What "origin-less" means (vs. traditional static hosting)

- Traditional: User → CDN → Origin Server (S3/Netlify/VPS) → Response
- Origin-less/Edge: User → Cloudflare Edge (R2 + Rules) → R2 (object) → Response

Benefits of origin-less:

- No centralized origin server to manage or scale
- Content served near users, reducing latency
- Built-in DDoS protection and global availability
- Pay for usage (storage and requests), not for idle servers

## Prerequisites

- Cloudflare account with R2 enabled and Cloudflare Rules configured (no Worker required)
- Wrangler CLI installed and authenticated
- A domain in Cloudflare DNS (optional but recommended)
- Your site's static build output (for SkyCMS, see "Deploying from SkyCMS" below)

## Step 1 — Create an R2 bucket

You can create R2 storage from the Cloudflare dashboard, or
you can use 'wrangler' as shown below.

```bash
npm install -g wrangler
wrangler login
wrangler r2 bucket create your-website-bucket

```
## Step 2 - Connect SkyCMS to R2

Cloudflare R2 is S3-compatible. With SkyCMS you'll provide your Account ID, bucket name, and S3-style credentials (Key ID/Secret).

Note: Cloudflare R2 uses a custom S3 endpoint (eg. `https://{account-id}.r2.cloudflarestorage.com`). For guidance on credentials and endpoint information, see [Cloudflare R2 access keys](../Configuration/Cloudflare-R2-AccessKeys.md).

Quick setup guide: see [Cloudflare R2 access keys](../Configuration/Cloudflare-R2-AccessKeys.md) to find your Account ID and bucket, and to generate an S3 API token (read/write/delete).

Format the connection string for R2 storage in the following manner. Note it requires
an Account ID, Bucket name, Key ID and Key Secret:

```json
{
   "ConnectionStrings": {
      "StorageConnectionString": "AccountId={Account ID};Bucket={bucket name};KeyId={access-key-id};Key={secret-access-key};"
   }
}
```

## Step 3 — Bind the R2 container to your domain

Open your R2 storage on the Cloudflare dashboard, choose "Settings" then "Custom Domains."

Near the top of the dialog, click '+ Add' button to the right of the title "Custom Domains."

Follow the dialog from that point forward.

## Step 4 — Create custom rules to handle root access

Open your domain on the Cloudflare dashboard, then find and expand "Rules" on the left edge of the dialog.

It is recommended that you create two rules:

1. Redirect http to https; and
2. Redirect root requests to index.html

There is a template for the first rule, go ahead and use that.

Create the second rule as a "custom filter expression" as shown below:

![Alt text](../cloudflarerule.png)

Next, under "Then..." add a "Rewrite to.." the value "index.html".

---

With this edge/origin-less approach, your site is globally distributed, highly performant, and free from the operational overhead of maintaining an origin server. R2 stores your files, Cloudflare Rules handle root and rewrite behaviors at the edge, and SkyCMS slots into your pipeline to publish updates reliably.
