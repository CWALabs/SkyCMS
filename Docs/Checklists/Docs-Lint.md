---
title: Documentation Lint Checklist
description: Checklist to enforce metadata and structure for SkyCMS docs
keywords: docs, lint, checklist, metadata, front-matter
audience: [contributors, editors, developers]
version: 1.0
last_updated: "2026-01-03"
stage: draft
read_time: 3
---

# Documentation Lint Checklist

Use this checklist before submitting doc changes to keep content AI/search-friendly and consistent.

## Required front matter
- [ ] `title`, `description` (1–2 sentences, plain language)
- [ ] `audience` list (e.g., ["developers", "administrators"])
- [ ] `tags/keywords` where applicable
- [ ] `version`, `last_updated` (YYYY-MM-DD), `stage` (draft|ga|deprecated), `read_time`
- [ ] `canonical` and `robots` when the page has mirrors or should be noindexed

## Required intro block (near top)
- [ ] Headings present: **When to use this**, **Why this matters**, **Key takeaways**, **Prerequisites**, **Quick path**
- [ ] Content is concise (bullets, plain language)

## Content hygiene
- [ ] Headings H2/H3 are scannable and action-oriented
- [ ] Code blocks include language hints; secrets redacted
- [ ] Links are stable and descriptive; avoid “click here”
- [ ] Images have alt text (or are removed if redundant)
- [ ] Avoid ambiguous pronouns; expand acronyms on first use

## SEO/AI signals
- [ ] Summary at top is short and specific
- [ ] Include key terms naturally (no stuffing)
- [ ] Add JSON-LD only where relevant (HowTo, FAQ, TechArticle) and keep minimal

## Accessibility
- [ ] Tables have headers; lists are ordered when sequence matters
- [ ] Avoid conveying info by color alone; include text cues

## Validation steps
- [ ] Run link check or spot-check new/changed links
- [ ] Verify examples/commands execute or are marked as examples
- [ ] Update `last_updated` and `read_time` after significant edits

## Optional
- [ ] Add FAQ if page has common failure modes
- [ ] Add Related links section for discoverability
