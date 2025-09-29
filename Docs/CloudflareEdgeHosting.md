# Cloudflare Edge Hosting: Origin-less Static Website Architecture

This guide shows how to host a static site on Cloudflare using an origin-less (edge) pattern with R2 object storage and a Worker acting as a smart proxy. It also explains how to configure SkyCMS to deploy your build output to R2.

Key idea: Unlike traditional static hosting that serves from a single origin, edge/origin-less sites are distributed and executed at Cloudflare’s global edge—improving latency, resilience, and cost profiles.

## What “origin-less” means (vs. traditional static hosting)

- Traditional: User → CDN → Origin Server (S3/Netlify/VPS) → Response
- Origin-less/Edge: User → Cloudflare Edge (Worker) → R2 (object) → Response

Benefits of origin-less:

- No centralized origin server to manage or scale
- Content served near users, reducing latency
- Built-in DDoS protection and global availability
- Pay for usage (storage and requests), not for idle servers

## Prerequisites

- Cloudflare account with R2 and Workers enabled
- Wrangler CLI installed and authenticated
- A domain in Cloudflare DNS (optional but recommended)
- Your site’s static build output (for SkyCMS, see “Deploying from SkyCMS” below)

## Step 1 — Create an R2 bucket

```bash
npm install -g wrangler
wrangler login
wrangler r2 bucket create your-website-bucket
```

## Step 2 — Configure the Worker and bind R2

Create `wrangler.toml` in your Worker project:

```toml
name = "website-proxy"
main = "src/index.js"
compatibility_date = "2025-01-01"

[[r2_buckets]]
binding = "BUCKET"
bucket_name = "your-website-bucket"
```

## Step 3 — Worker proxy for origin-less static hosting

```javascript
// src/index.js
export default {
  async fetch(request, env, ctx) {
    const url = new URL(request.url);
    let key = url.pathname.slice(1); // remove leading /

    // Directory and root handling
    if (key === '' || key.endsWith('/')) key += 'index.html';

    // Clean URLs → add .html
    if (!key.includes('.') && !key.endsWith('/')) key += '.html';

    try {
      const object = await env.BUCKET.get(key);

      if (!object) {
        // SPA/404 fallback
        const fallback = await env.BUCKET.get('404.html') || await env.BUCKET.get('index.html');
        if (!fallback) return new Response('Not Found', { status: 404 });
        return new Response(fallback.body, {
          headers: { 'Content-Type': 'text/html', 'Cache-Control': 'public, max-age=300' }
        });
      }

      const headers = new Headers();
      headers.set('Content-Type', getContentType(key));
      headers.set('Cache-Control', getCacheControl(key));
      if (object.etag) headers.set('ETag', object.etag);

      return new Response(object.body, { headers });
    } catch (err) {
      return new Response('Internal Server Error', { status: 500 });
    }
  }
}

function getContentType(key) {
  const map = {
    html: 'text/html', css: 'text/css', js: 'application/javascript',
    json: 'application/json', png: 'image/png', jpg: 'image/jpeg',
    jpeg: 'image/jpeg', gif: 'image/gif', svg: 'image/svg+xml',
    woff: 'font/woff', woff2: 'font/woff2', ico: 'image/x-icon'
  };
  const ext = key.split('.').pop().toLowerCase();
  return map[ext] || 'application/octet-stream';
}

function getCacheControl(key) {
  return /\.(css|js|png|jpe?g|gif|svg|woff2?)$/.test(key)
    ? 'public, max-age=31536000, immutable'
    : 'public, max-age=300';
}
```

Deploy and route the Worker:

```bash
wrangler deploy
wrangler route create "yourdomain.com/*" website-proxy
```

## Step 4 — Upload your static site to R2

You can upload with the Cloudflare dashboard, the Wrangler CLI, or a CI/CD pipeline. With Wrangler:

```bash
# Upload a local folder recursively into the bucket root
wrangler r2 object put your-website-bucket --recursive --file=./dist
```

## Deploying from SkyCMS (R2)

SkyCMS can produce static output that you’ll deploy to R2 as part of your pipeline. There are two integration options today:

1. Recommended (CI/CD via Wrangler)

- Build your site with SkyCMS
- Use Wrangler to push the build output to R2
- Optionally deploy the Worker and purge cache

Example GitHub Actions workflow:

```yaml
name: Deploy to Cloudflare Edge
on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Restore & Build (SkyCMS)
        run: |
          dotnet restore
          dotnet build --configuration Release

      - name: Publish static site
        run: |
          # Adjust to your static export step/output folder
          # e.g., export to ./dist
          echo "Ensure your build outputs to ./dist"

      - name: Install Wrangler
        run: npm i -g wrangler

      - name: Upload to R2
        run: wrangler r2 object put ${{ vars.R2_BUCKET }} --recursive --file=./dist
        env:
          CLOUDFLARE_API_TOKEN: ${{ secrets.CLOUDFLARE_API_TOKEN }}

      - name: Deploy Worker
        run: wrangler deploy
        env:
          CLOUDFLARE_API_TOKEN: ${{ secrets.CLOUDFLARE_API_TOKEN }}

      - name: Purge Cache (optional)
        run: |
          curl -X POST "https://api.cloudflare.com/client/v4/zones/${{ secrets.CLOUDFLARE_ZONE_ID }}/purge_cache" \
            -H "Authorization: Bearer ${{ secrets.CLOUDFLARE_API_TOKEN }}" \
            -H "Content-Type: application/json" \
            --data '{"purge_everything":true}'
```

1. Direct S3-compatible publish (when endpoint is supported)

Cloudflare R2 is S3-compatible but requires a custom S3 endpoint like `https://<account-id>.r2.cloudflarestorage.com`. The current SkyCMS storage driver doesn’t expose a custom endpoint (ServiceURL) yet. Until that’s added, prefer the CI/CD approach above.

For credentials and account info, see: `Docs/Cloudflare-R2-AccessKeys.md`.

## Optional: Project config template

If your project uses a YAML config to describe build/deploy settings, this example shows the core values you’ll need for origin-less R2 hosting:

```yaml
site:
  name: "My Edge Website"
  url: "https://example.com"

build:
  output_dir: "dist"
  
deployment:
  provider: "cloudflare-r2"
  bucket: "my-edge-website"
  region: "auto"
  
cloudflare:
  account_id: "your-cloudflare-account-id"
  api_token: "your-cloudflare-api-token"
  worker_name: "edge-website-worker"
  
content:
  collections:
    - name: "pages"
      folder: "content/pages"
      create: true
      fields:
        - { name: "title", label: "Title", widget: "string" }
        - { name: "body", label: "Body", widget: "markdown" }
```

## Tips and troubleshooting

- 404/SPA routing: Include `404.html` or rely on `index.html` as a fallback in the Worker.
- Cache control: Use long-lived caching for versioned assets and short TTL for HTML.
- MIME types: Ensure the Worker sets `Content-Type` correctly to avoid rendering issues.
- DNS/SSL: Point your domain to Cloudflare and route it to the Worker for full edge delivery.
- R2 credentials: Follow `Docs/Cloudflare-R2-AccessKeys.md` to create least-privilege tokens.

---

With this edge/origin-less approach, your site is globally distributed, highly performant, and free from the operational overhead of maintaining an origin server. R2 stores your files, the Worker serves them intelligently at the edge, and SkyCMS slots into your pipeline to publish updates reliably.
