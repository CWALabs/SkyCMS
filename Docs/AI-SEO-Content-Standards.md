---
title: AI & SEO Content Standards
description: Front matter, anchors, chunking, and JSON-LD conventions to keep SkyCMS docs ingestion-friendly for AI and search engines
keywords: ai, seo, documentation, front-matter, json-ld, anchors, chunking
audience: [developers, writers, editors]
version: 2.0
updated: 2025-12-20
canonical: /AI-SEO-Content-Standards.html
aliases: []
scope:
  platforms: [all]
  tenancy: [all]
status: stable
chunk_hint: 400
---

# AI & SEO Content Standards {#ai-seo-content-standards}

Use this checklist to keep docs stable for AI embeddings and search.

## Front matter contract {#front-matter-contract}

Required fields:
- `title`, `description`, `keywords`, `audience` (array), `version`, `updated` (YYYY-MM-DD)
- `canonical` (page URL) and `aliases` (old slugs/paths)
- `scope` object: `platforms` (e.g., azure, aws, local, multi-cloud) and `tenancy` (single, multi)
- `status`: draft | beta | stable | deprecated
- `chunk_hint`: estimated tokens per logical chunk (200-400 recommended)

## Anchor conventions {#anchor-conventions}

- Add explicit IDs to every H2/H3 that should be linkable or chunked: `## Installation {#installation}`
- Keep IDs short, lowercase, hyphenated; do not change them after publishing.
- Use role/scope in IDs when helpful: `#azure-setup`, `#multi-tenant-steps`.

## Chunking guidance {#chunking-guidance}

- Keep sections single-purpose; avoid mixing platforms in one chunk.
- Target 200-400 tokens per chunk; keep tables and procedure steps intact.
- Start sections with a lead sentence that restates context so chunks stand alone.

## Key facts and Q&A {#key-facts}

- Add a short **Key facts** block per page (3-6 bullets).
- Add **FAQ** with 3-7 concise Q&A items that map to real queries.

## JSON-LD patterns {#json-ld}

Embed structured data in pages that have procedures or FAQs:
- Use `FAQPage` for Q&A blocks.
- Use `HowTo` for stepwise procedures (QuickStart, setup guides).
- Place `<script type="application/ld+json">` blocks at the bottom of the page; keep arrays small and accurate.

Example FAQ stub:
```json
{
  "@context": "https://schema.org",
  "@type": "FAQPage",
  "mainEntity": [
    {"@type": "Question", "name": "Question?", "acceptedAnswer": {"@type": "Answer", "text": "Answer."}}
  ]
}
```

Example HowTo stub:
```json
{
  "@context": "https://schema.org",
  "@type": "HowTo",
  "name": "Do the thing",
  "step": [
    {"@type": "HowToStep", "name": "Step 1", "text": "What to do."}
  ]
}
```

## Cross-linking and scope {#cross-linking}

- Cross-link related setup, troubleshooting, and reference pages at the end of each section.
- Call out platform scope at the start of any platform-specific steps.

## Validation checklist {#validation-checklist}

- Front matter present and complete
- Anchors set for key sections
- Key facts + FAQ present
- JSON-LD included where applicable
- Links tested; code blocks copy/paste-ready
