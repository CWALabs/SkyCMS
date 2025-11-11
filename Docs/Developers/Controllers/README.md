# Controllers Documentation

This section contains developer documentation for core controllers shared across SkyCMS applications.

- [HomeControllerBase](HomeControllerBase.md) — Common endpoints used by Publisher and Editor.
- [PubControllerBase](PubControllerBase.md) — Secure publisher file proxy and access control base.

If you add new shared controllers, create a new markdown file here and link it in this index.

Last updated: October 2025

---

## Publisher HTTP endpoints overview

This table summarizes the primary HTTP endpoints exposed by the Publisher app, showing how the base controllers are used. Routes are configured in `Publisher/Boot/DynamicPublisherWebsite.cs`.

| Method | Path pattern | Controller/Action | Source | Description | Auth/Caching |
|--------|--------------|-------------------|--------|-------------|--------------|
| GET | `pub/{*index}` | `PubController.Index()` | [PubControllerBase](PubControllerBase.md) | Secure file proxy for assets in storage (e.g., `/pub/articles/{articleNumber}/...`) | If `CosmosRequiresAuthentication` is true, requires auth; per-article authorization enforced under `/pub/articles/`; `Expires` set to now |
| GET | `{controller=Home}/{action=Index}/{id?}` | `HomeController.Index()` | Derived from HomeControllerBase | Render a published page by URL (default route) | Public or gated by `CosmosRequiresAuthentication`; sets cache headers accordingly |
| HEAD | any page path | `HomeController.CCMS___Head()` (ActionName: `Index`) | Derived from HomeControllerBase | Lightweight header response for published page (ETag/Last-Modified/Expires) | Public if site not requiring auth; otherwise `401` |
| GET | `.well-known/microsoft-identity-association.json` | `HomeController.GetMicrosoftIdentityAssociation()` | Publisher HomeController | Microsoft identity association document | Public |
| GET | `Home/CCMS_GetArticleFolderContents` | `HomeControllerBase.CCMS_GetArticleFolderContents()` | [HomeControllerBase](HomeControllerBase.md) | List article folder contents in blob storage | Depends on caller context; resolves article by Referer/URL |
| GET | `Home/GetTOC` | `HomeControllerBase.GetTOC()` | [HomeControllerBase](HomeControllerBase.md) | Table of contents for a given UrlPath | CORS policy `AllCors` |
| POST | `Home/CCMS_POSTCONTACT_INFO` | `HomeControllerBase.CCMS_POSTCONTACT_INFO()` | [HomeControllerBase](HomeControllerBase.md) | Submit contact form | Anti-forgery token required; rate-limited |
| POST | `Home/CCMS___SEARCH` | `HomeControllerBase.CCMS___SEARCH()` | [HomeControllerBase](HomeControllerBase.md) | Search published articles | Public POST |
| GET | `Home/CCMS_UTILITIES_NET_PING_HEALTH_CHECK` | `HomeControllerBase.CCMS_UTILITIES_NET_PING_HEALTH_CHECK()` | [HomeControllerBase](HomeControllerBase.md) | Health check (DB connectivity) | `AllowAnonymous`; rate-limited |

Notes:

- Fallback routing maps any unmatched path to `Home/Index`, which resolves and renders the corresponding published page.
- In Editor, a similar `pub/{*index}` route is mapped to the Editor’s `PubController` which also derives from `PubControllerBase`.

