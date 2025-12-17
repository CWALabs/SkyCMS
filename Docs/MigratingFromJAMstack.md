---
title: Migrating from JAMstack to SkyCMS
description: Guide for migrating from Git-based static site workflows (Netlify, Vercel, GitHub Pages) to SkyCMS
keywords: migration, JAMstack, Netlify, Vercel, GitHub-Pages, static-sites
audience: [developers, decision-makers]
---

# Migrating from Git-Based Static Site Deployment to SkyCMS

## From Netlify/Vercel/GitHub Pages to SkyCMS

### What You're Leaving Behind
- Git-based content management
- Build pipeline configuration (GitHub Actions, Netlify Build)
- Static site generators (Jekyll, Hugo, Gatsby, Next.js)
- Complex preview deployment setups

### What You're Gaining
- Visual content editing (no Git required for content team)
- Instant publishing (no build delays)
- Hybrid static + dynamic content
- Built-in version control
- Integrated media management
- Edge-native hosting (Cloudflare R2 + Rules, origin-less deployment)
- Multi-cloud storage support (Azure Blob, AWS S3, Cloudflare R2)
- Single integrated Publisher – no external static site generator or CI/CD pipeline required
- Page scheduling and instantaneous rollbacks with version history

### Migration Steps

1. Export or convert existing content

    - Export markdown, images and metadata from your current site (Jekyll, Hugo, Gatsby, etc.)

    - When exporting content from Git-backed sites, preserve frontmatter and permalink settings if possible
2. Install SkyCMS and choose a deployment mode

    - Try locally with Docker: `docker run -d -p 8080:8080 toiyabe/sky-editor:latest`

    - Or deploy to cloud with ARM templates / container orchestrator (see `ArmTemplates/README.md`)
3. Configure storage and CDN

    - Choose Azure Blob, AWS S3, or Cloudflare R2; see `Docs/Configuration/Storage-Configuration-Reference.md`

    - If you want origin-less edge hosting, follow `Docs/Installation/CloudflareEdgeHosting.md`
4. Import content into the Editor

    - Bulk import Markdown or HTML pages using the Editor import tools or a one-off script

    - Verify images and attachments are moved into the `Blob Service`
5. Define templates and reusable components

    - Map existing page templates to SkyCMS template regions

    - Recreate dynamic features with shortcodes or embedded scripts if needed
6. Preview and schedule publication

    - Use built-in preview mode to validate pages before publishing

    - Use the scheduling calendar to time live content
7. Publish and update DNS

    - Publish directly to your chosen storage provider; no external build steps

    - Point your DNS to the CDN or configure Cloudflare DNS + Rules for origin-less distribution

### Common Scenarios

- **From Jekyll** → Convert posts and pages preserving frontmatter; use Editor import or script

- **From Hugo** → Convert shortcodes to SkyCMS template snippets and import content

- **From Gatsby/Next.js** → Recreate dynamic components with SkyCMS templates, use API mode for headless parts

- **From Eleventy** → Import pages and rebuild templates using GrapesJS + Monaco editor

### Considerations & Tips

- Template mapping: Jet engine and component-based templates may need manual recreation; reuse static assets and partials where possible

- Assets and media: Move images to Blob Service early to take advantage of on-upload processing and CDN delivery

- Headless/API mode: Use the optional REST API for mobile apps or third-party integrations; SkyCMS remains page-first

- Versioning and rollbacks: Versions are stored in the CMS—quickly restore any published page without a full rebuild

- Scheduling: Built-in scheduling means no cron or CI/CD for timed content

- Licensing: Remember the default CKEditor 5 license (GPL 2.0 or commercial upgrade) — see `LICENSE-GPL` and `LICENSE-MIT`

### Useful Links

- Storage & Deployment: `Docs/Configuration/Storage-Configuration-Reference.md`

- Edge Hosting (Cloudflare): `Docs/Installation/CloudflareEdgeHosting.md`

- Publishing & Templates: `Publisher/README.md`

- Editors & Tools: `Docs/Editors/LiveEditor/README.md`
