# SkyCMS Blog Feature – Proposed Enhancements

## 1. Listing (Blog Index)
- Path: `/blog`
- Data Source: `ArticleCatalog` joined to latest `Article` filtering `ArticleType = BlogPost` and `Published != null`
- Fields: Title, Published date, Introduction, BannerImage, Category, UrlPath
- Pagination: Skip/Take with query parameters `?page=n`
- SEO: `<link rel="canonical">`, structured data (`BlogPosting` schema)

## 2. Category Archives
- Path: `/blog/category/{category-slug}`
- Slug Strategy: lowercase + `-` replace spaces
- Query: same as index plus `Category == category-slug`
- Empty Category: return 404 or soft “No posts yet.”

## 3. RSS Feed
- Path: `/blog/rss`
- Format: RSS 2.0 (or Atom)
- Items: Most recent N (configurable, default 20)
- Fields: Title, Link, PubDate, Description (Introduction), GUID (ArticleNumber + version or UrlPath)
- Cache: In-memory 5–15 minutes; bust on publish event.

## 4. Sitemap Integration
- Extend existing sitemap generator to include published BlogPosts
- Priority weighting vs static pages (e.g. 0.6)
- Changefreq heuristic based on recent publish cadence

## 5. Tagging System (Optional Extension)
- New table: `ArticleTag { ArticleNumber, Tag }`
- UI: Multi-select token input on create/edit (only for BlogPost)
- Indexing: Add `Tags` array projection in catalog (optional denormalization)

## 6. Search Improvements
- Add scoped search endpoint `/blog/search?term=`
- Full-text across Title + Introduction; optional body include
- Return lightweight DTO

## 7. Performance / Caching
| Layer | Strategy |
|-------|----------|
| Catalog queries | Server-side memory cache keyed by (page, category) with dependency invalidation on publish/unpublish. |
| RSS | Cache feed XML string; regenerate on publish. |
| Banner images | Enforce width/height to reduce layout shift. |

## 8. Editor UX Upgrades
| Feature | Description |
|---------|-------------|
| Live preview card | Show how post appears in index (Title + intro + image). |
| Category normalization | Lowercase & trim spaces on save. |
| Auto-slug Category | Derive slug form stored (save both raw + slug optional). |
| Draft badge | Indicate unpublished status in Versions & index preview. |

## 9. Analytics Hooks
- Event: “BlogPostPublished” (ArticleNumber, Category, PublishedUtc)
- Optional injection: structured data for each blog post page (JSON-LD)

## 10. Security / Permissions
- If `ArticlePermissions` populated, exclude from public index & RSS
- Option: Add “PrivatePost” badge internally

## 11. API Endpoints (Future)
| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/blog/posts` | GET | Paginated published blog posts |
| `/api/blog/post/{slug}` | GET | Single published post |
| `/api/blog/categories` | GET | List distinct categories with counts |
| `/api/blog/rss` | GET | RSS feed |

## 12. Minimal Implementation Order
1. Blog index Razor Page
2. RSS feed
3. Category pages
4. Pagination & canonical tags
5. Caching layer
6. Tagging system (if required)
7. Search endpoint
8. Structured data + analytics events

## 13. Non-Goals (Short Term)
- WYSIWYG semantic cleanup refactor
- Commenting platform
- Full-blown taxonomy hierarchy
- Multi-author attribution UI (AuthorInfo already supports simple data)

## 14. Risks & Mitigations
| Risk | Mitigation |
|------|------------|
| Large catalog query cost | Project only needed fields; index on ArticleType + Published. |
| Category explosion (typos) | Normalize + optional admin-managed allowed list. |
| RSS size growth | Limit item count, enforce intro length. |
| Duplicate slugs after title edits | Existing redirect logic already in place. |

## 15. Publish Event Hook (Example Skeleton)

## 16. Development Considerations
- Leverage existing `CatalogEntry` infrastructure for Blog-specific enhancements.
- Plan phased delivery with iterative testing, starting with index and RSS feed.

## 17. Analytics and Monitoring
- Implement event tracking for blog interactions (e.g., views, shares).
- Monitor performance metrics pre- and post-implementation to gauge impact.

## 18. Future Possibilities
- Explore AI-generated content summaries for improved SEO and engagement.
- Investigate personalized content recommendations based on user behavior.

## 19. Testing Checklist
| Area | Test |
|------|------|
| Creation | BlogPost type shows extra fields |
| Slug collisions | Title change creates redirect & preserves navigation |
| RSS | Valid XML, correct `pubDate`, item count |
| Pagination | Deterministic ordering by Published DESC |
| Security | Private posts excluded from index & RSS |
| Performance | Query under target latency with N=1000 posts |

---

### Summary
Enhancements can be layered without schema changes by leveraging `CatalogEntry` + filtered queries. Initial delivery should focus on index + RSS to make existing metadata consumable publicly.