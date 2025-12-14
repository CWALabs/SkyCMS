{% include nav.html %}

# SkyCMS Blog Post Lifecycle (Current Implementation)

## Status
There is no distinct “Blog” module yet. A blog post is an `Article` whose `ArticleType` is set to the enum value representing a blog (e.g. `BlogPost`). Listing, category pages, and RSS are not yet implemented—only the underlying article infrastructure exists.

---

## Core Data Objects Involved

| Concern | Entity / Source | Notes |
|---------|-----------------|-------|
| Draft & versions | `Article` | One row per version. Latest unpublished version is edited. |
| Published snapshot | `PublishedPage` | Single active published instance per `ArticleNumber`. |
| Listing metadata | `CatalogEntry` | Title, Introduction, Category, BannerImage, Published date, Permissions. |
| Blog classification | `Article.ArticleType`, `Article.Category`, `Article.Introduction` | Category is a single string; Introduction used as teaser. |
| Templates | `Template` | Optional starting HTML with editable regions (`data-ccms-ceid`). |
| Layout | `Layout` | Surrounding site chrome. |
| Permissions | `CatalogEntry.ArticlePermissions` | Roles/users allowed to view (when set). |

---

## Creation Workflow (User Journey)

1. Navigate to: Editor → Create (`GET /Editor/Create`)
2. Fill form (`Create.cshtml`):
   - Title (used to derive URL slug)
   - Optional Template selection
   - Select `ArticleType` = BlogPost (reveals Category + Introduction fields)
   - Category (taxonomy label)
   - Introduction (teaser / excerpt)
3. Client validation (character restrictions, reserved paths)
4. Submit (`POST /Editor/Create`)
5. Server flow (`EditorController.Create`):
   - Validates uniqueness & reserved path via `ArticleEditLogic.ValidateTitle`
   - Calls `ArticleEditLogic.CreateArticle`
     - Assigns next `ArticleNumber`
     - Injects template content (ensures editable regions marked)
     - Sets `UrlPath = Normalize(Title)` (unless first page → `root`)
     - First site page auto-publishes; others start unpublished
   - Sets blog metadata (ArticleType / Category / Introduction)
   - Persists via `ArticleEditLogic.SaveArticle` (which:
     - Normalizes content
     - Updates `CatalogEntry`
   - Redirects to Versions list (`/Editor/Versions/{ArticleNumber}`)

Result: A draft blog post (unpublished unless it happened to be the very first site page).

---

## Editing Paths

| Editor Type | Entry Point | Purpose |
|-------------|-------------|---------|
| Live (inline) | `/Editor/Edit/{ArticleNumber}` | Region-level updates (contenteditable). |
| Code (Monaco) | `/Editor/EditCode/{ArticleNumber}` | Full HTML + head/footer JS editing. |
| Designer (GrapeJS) | `/Editor/Designer/{ArticleNumber}` | Visual composition when template supports it. |

All eventually invoke `ArticleEditLogic.SaveArticle`.

### Save Mechanics
- Editable region update → `EditSaveRegion` (replaces single `data-ccms-ceid` region)
- Full body update → `Edit` / `EditCode`
- Designer → `Designer` POST assembles designer HTML + inline CSS into article Content
- Each save:
  - Re-marks editable regions
  - Handles potential title changes (slug, redirect management)
  - Updates `CatalogEntry`
  - Leaves `Published` null unless explicitly published

---

## Versioning

| Action | Behavior |
|--------|----------|
| Editing a published article | A new unpublished version is cloned automatically (`GetArticleForEdit` creates new version). |
| Manual new version | “Create Version” duplicates chosen version with incremented `VersionNumber`. |
| Catalog entry | Always reflects latest (top) version state (published or not). |
| Published snapshot | Only one per logical article; republished version replaces prior snapshot. |

---

## Publishing Flow

1. User triggers publish (from editor UI / Publish dialog).
2. Controller (`PublishPage`) calls `ArticleEditLogic.PublishArticle`.
3. Logic pipeline:
   - Sets `Published` timestamp (or scheduled time)
   - Creates / replaces `PublishedPage`
   - Generates static file (if static mode on)
   - Refreshes `CatalogEntry`
   - CDN purge (if configured)
4. Listing metadata now has a concrete Published date.

---

## Unpublishing
- `EditorController.UnpublishPage(int articleNumber)` delegates to `ArticleEditLogic.UnpublishArticle`.
- Removes published snapshot & static artifact; catalog remains (Published becomes null on further edits).

---

## Deletion & Restoration

| Operation | Effect |
|-----------|--------|
| Trash (`DeleteArticle`) | Flags all versions `StatusCode = Deleted`, removes snapshot, deletes catalog entry, static file. |
| Restore (`RestoreArticle`) | Resets status to Active, ensures unique Title/Url if conflict, re-adds catalog entry (unpublished). |
| Hard delete | Not currently implemented (soft delete only). |

---

## Data Flow Summary

---

## Key Touchpoints (Methods)

| Purpose | Method |
|---------|--------|
| Title validation | `ArticleEditLogic.ValidateTitle` |
| Creation | `ArticleEditLogic.CreateArticle` |
| Saving edits | `ArticleEditLogic.SaveArticle` |
| Publishing | `ArticleEditLogic.PublishArticle` |
| Unpublish | `ArticleEditLogic.UnpublishArticle` |
| Catalog sync | `UpsertCatalogEntry` (internal) |
| Static page generation | `CreateStaticWebpage` |
| Version clone (auto) | `EditorController.GetArticleForEdit` |
| Version clone (manual) | `EditorController.CreateVersion` |
| Delete / Restore | `DeleteArticle`, `RestoreArticle` |

---

## UX Notes
- Blog-specific fields only appear client-side when `ArticleType == BlogPost`.
- Introduction auto-generated (first paragraph) if left blank during catalog upsert.
- Editable region markers (`data-ccms-ceid`) retained through template application and saves.

---

## Risks / Edge Cases
| Case | Mitigation |
|------|------------|
| Title changed after publish | Slug/redirect logic handled by `titleChangeService`. |
| Duplicate category casing | No normalization layer; recommend enforcing lowercase UI. |
| Large introduction text | Truncated to 512 chars in catalog generation. |
| Simultaneous editors | Basic optimistic handling via row updates; no merge UI. |

---

## Quick Enhancement Roadmap (Incremental)
1. Blog index Razor Page using `ArticleCatalog`.
2. RSS feed builder (top N published blog posts).
3. Category listing pages (`/blog/category/{slug}`).
4. Pagination & canonical link headers.
5. Faceted filtering (Category, date range).
6. Multi-tag support (new join table).
7. Comment provider integration (pluggable).
8. Sitemap augmentation for blog posts.

---

## Summary
The “blog” feature today is a thin specialization of the generic Article system leveraging existing versioning, publishing, and catalog infrastructure. Surface additions (index, feeds, archives) can be layered without schema changes by querying `ArticleCatalog` filtered on `ArticleType` and `Published != null`.
