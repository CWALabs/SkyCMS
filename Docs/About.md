---
title: About SkyCMS
description: Learn about SkyCMS, an edge-native CMS that combines traditional editor usability with static site generation speed
keywords: about, overview, edge-native, CMS, Docker, multi-cloud
audiencе: [decision-makers, developers, content-creators]
---

# About SkyCMS

SkyCMS is an edge-native CMS that combines the usability of traditional editors with the speed of static site generation—without CI/CD or Git pipelines. It ships as Docker containers and deploys to any cloud that supports S3-compatible storage or Azure Blob, with optional origin-less edge hosting on Cloudflare R2 + Rules.

## Who It’s For
- Decision makers: Lower hosting costs (often $0–10/month at scale), minimal ops overhead, and faster time-to-publish (<5 seconds) versus typical JAMstack pipelines.
- Developers: No external build chain required; integrated Publisher handles rendering and deployment. Multi-cloud by default; supports Cosmos DB, SQL Server, MySQL, or SQLite plus S3/Azure/R2 storage.
- Editors & content teams: Familiar WYSIWYG (CKEditor 5), visual builder (GrapesJS), and code editor (Monaco) with versioning, scheduling, and one-click publishing.

## What Makes It Different
- Integrated publishing pipeline: Renders pages and deploys directly to storage/CDN—no Netlify/Vercel-style builds or Git workflows.
- Hybrid delivery: Serve static pages for speed while keeping dynamic capabilities when needed.
- Edge-focused: Origin-less option on Cloudflare R2 + Rules; global CDN distribution out of the box.
- Instant publishing: Changes go live in seconds; built-in version control and rollbacks.
- Complete CMS, not just headless: Roles, media management, templates, and page scheduling are included.

## Core Applications
- Editor: Authoring experience with CKEditor 5, GrapesJS, Monaco; asset management; multi-tenant or single-tenant setup wizard.
- Publisher: Renders and serves pages in dynamic or static proxy mode; SEO-friendly, cached, and CDN-ready.
- Shared services: Blob service for multi-cloud storage, dynamic configuration for multi-tenant setups, flexible identity via ASP.NET Core Identity.

## Deployment & Hosting Options
- Static mode (primary): Pre-rendered HTML to Azure Blob, S3, or Cloudflare R2.
- Edge/origin-less: Cloudflare R2 + Rules (no Worker needed).
- Dynamic or decoupled: Run Publisher for server-side rendering; optional API mode for headless scenarios.
- Containers: toiyabe/sky-editor, toiyabe/sky-publisher, toiyabe/sky-api on Docker Hub.

## Editing & Workflow Highlights
- Rich tools: CKEditor 5 for text, GrapesJS for drag-and-drop layouts, Monaco for code, Filerobot for images.
- Scheduling & versioning: Draft → versioned → publish with restore; calendar-based scheduling.
- CDN integration: Automatic cache purge hooks for Cloudflare, Azure CDN, and Sucuri.
- Media management: File manager with per-page asset folders and S3/Azure/R2 abstraction.

## Performance & Reliability
- Static delivery from CDN/edge; typical static responses around ~50ms.
- Caching (memory + distributed), optional rate limiting, and telemetry via Application Insights.
- Designed to handle traffic spikes with minimal infrastructure.

## Security & Identity
- ASP.NET Core Identity with roles; Google/Microsoft OAuth; optional Azure B2C.
- Secure storage access and antiforgery protections.
- Configurable auth enforcement for publishing workflows.

## Licensing
Dual-licensed: GPL 2.0-or-later and MIT (see project licenses).

## Get Started
- Overview & docs: Docs/README.md
- Quick start: Docs/QuickStart.md
- Deployment: Docs/AzureInstall.md, Docs/S3StaticWebsite.md, Docs/CloudflareEdgeHosting.md
- Component guides: Editor/README.md, Publisher/README.md, Common/README.md
