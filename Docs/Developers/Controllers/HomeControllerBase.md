---
title: HomeControllerBase Developer Documentation
description: Technical reference for the shared HomeControllerBase class providing common endpoints for Editor and Publisher
keywords: HomeControllerBase, controller, API, developer-reference
audience: [developers]
---

# HomeControllerBase Developer Documentation

Location: `Common/HomeControllerBase.cs`

The `HomeControllerBase` class provides shared endpoints and utilities used by both SkyCMS Editor and Publisher applications. Derived controllers (e.g., `Publisher/Controllers/HomeController`) inherit this base to expose common functionality for content discovery, search, contact submissions, and health checks.

- Namespace: `Cosmos.Common`
- Inherits: `Microsoft.AspNetCore.Mvc.Controller`
- Consumed by: `SkyCMS.Publisher`, `SkyCMS.Editor`

## Dependencies (via constructor)

```csharp
public HomeControllerBase(
    ArticleLogic articleLogic,
    ApplicationDbContext dbContext,
    StorageContext storageContext,
    ILogger logger,
    IEmailSender emailSender)
```

- `ArticleLogic` — Core domain logic for articles (TOC, search)
- `ApplicationDbContext` — EF Core DB context used for pages and health checks
- `StorageContext` — Blob/file storage context for article assets
- `ILogger` — Logging
- `IEmailSender` — Email infrastructure used by contact form processing

---

## Endpoints

Below endpoints are exposed by derived controllers that inherit this class. The final route is determined by the derived controller's route configuration (typically `/{Controller}/{Action}`), so examples use `/Home/{Action}`.

### 1) CCMS_GetArticleFolderContents

Gets file/folder entries within an article-specific folder in blob storage.

```csharp
public Task<IActionResult> CCMS_GetArticleFolderContents(string path = "")
```

- HTTP Verb: GET
- Auth: Inherits auth from derived controller (typically public for reading assets)
- Rate limiting: None
- Parameters:
  - `path` (string, optional): Sub-path under the article's root folder (e.g., `images/headers`)
- Behavior:
  - Resolves the current `articleNumber` from the HTTP Referer (editor context) or from the URL path by looking up the page by `UrlPath`.
  - Uses `CosmosUtilities.GetArticleFolderContents(storageContext, articleNumber, path)` to fetch entries.
- Returns: `JsonResult` with folder entries
- Status codes:
  - 200 with JSON payload on success
  - 400 if `ModelState` invalid
  - 404 if the article cannot be resolved

Example (browser context):

```js
// Invoked from an editor-loaded page where Referer header is set automatically
fetch('/Home/CCMS_GetArticleFolderContents?path=images')
  .then(r => r.json())
  .then(console.log);
```

Notes:

- When calling outside of the editor iframe, ensure the Referer reflects a valid page URL so the article can be resolved.

---

### 2) GetTOC

Returns the children (table of contents) for a given page path.

```csharp
[EnableCors("AllCors")]
public Task<IActionResult> GetTOC(string page, bool? orderByPub, int? pageNo, int? pageSize)
```

- HTTP Verb: GET
- CORS: Enabled with policy `AllCors`
- Auth: Inherits from derived controller
- Parameters:
  - `page` (string, required): `UrlPath` of the parent page (e.g., `/blog`)
  - `orderByPub` (bool?, optional): Order by publish date when true; default false
  - `pageNo` (int?, optional): Zero-based page number; default 0
  - `pageSize` (int?, optional): Items per page; default 10
- Behavior:
  - Delegates to `articleLogic.GetTableOfContents(page, pageNo ?? 0, pageSize ?? 10, orderByPub ?? false)`
- Returns: `JsonResult` with TOC items
- Status codes:
  - 200 with JSON payload on success
  - 400 if `ModelState` invalid

Example:

```bash
curl "https://example.com/Home/GetTOC?page=/blog&orderByPub=true&pageNo=0&pageSize=10"
```

---

### 3) CCMS_POSTCONTACT_INFO

Creates a contact record and optionally sends notifications.

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
[EnableRateLimiting("fixed")]
public Task<IActionResult> CCMS_POSTCONTACT_INFO(ContactViewModel model)
```

- HTTP Verb: POST
- Auth: Inherits from derived controller (often public)
- Anti-forgery: Required (`[ValidateAntiForgeryToken]`)
- Rate limiting: Policy `fixed`
- Body: `ContactViewModel` (required)
- Behavior:
  - Assigns `Id`, `Created`, and `Updated` timestamps
  - On valid model, uses `Services.ContactManagementService(dbContext, emailSender, logger, HttpContext).AddContactAsync(model)`
- Returns: `JsonResult` with operation outcome
- Status codes:
  - 200 with JSON payload on success
  - 400 if model state invalid
  - 404 if `model` is null

Client notes:

- For AJAX calls, include the anti-forgery token in the request header/body per ASP.NET Core conventions.

Example (razor form):

```html
<form asp-action="CCMS_POSTCONTACT_INFO" method="post">
  <input name="Name" />
  <input name="Email" />
  <textarea name="Message"></textarea>
  <input name="__RequestVerificationToken" type="hidden" value="@Antiforgery.GetTokens(HttpContext).RequestToken" />
  <button type="submit">Send</button>
</form>
```

---

### 4) CCMS___SEARCH

Searches published articles by keyword or phrase.

```csharp
[HttpPost]
public Task<IActionResult> CCMS___SEARCH(string searchTxt, bool? includeText = null)
```

- HTTP Verb: POST
- Auth: Inherits from derived controller (typically public)
- Parameters:
  - `searchTxt` (string, required): Search term
  - `includeText` (bool?, optional): Currently not used (reserved for future enhancements)
- Behavior:
  - Validates non-empty `searchTxt`
  - Delegates to `articleLogic.Search(searchTxt)`
- Returns: `JsonResult` with search results
- Status codes:
  - 200 with JSON payload on success
  - 400 if `ModelState` invalid or `searchTxt` missing

Example:

```bash
curl -X POST -d "searchTxt=cloud" https://example.com/Home/CCMS___SEARCH
```

---

### 5) CCMS_UTILITIES_NET_PING_HEALTH_CHECK

Application health check endpoint.

```csharp
[AllowAnonymous]
[EnableRateLimiting("fixed")]
public Task<IActionResult> CCMS_UTILITIES_NET_PING_HEALTH_CHECK()
```

- HTTP Verb: GET
- Auth: Anonymous allowed
- Rate limiting: Policy `fixed`
- Behavior:
  - Returns `200 OK` if the application can connect to the database
  - Returns `500` if the connectivity check fails
- Returns: Empty body with appropriate status
- Status codes:
  - 200 when healthy
  - 400 if `ModelState` invalid (edge-case)
  - 500 when DB connectivity check throws

Example:

```bash
curl -I https://example.com/Home/CCMS_UTILITIES_NET_PING_HEALTH_CHECK
```

---

## Internal Utilities

### GetArticleNumberFromRequestHeaders (private)

Resolves the `ArticleNumber` for the current request by inspecting the Referer URL or the current path.

```csharp
private Task<int?> GetArticleNumberFromRequestHeaders()
```

Resolution order:

1. If Referer query contains `articleNumber`, parse it
2. Else, if Referer path contains `editor/ccmscontent`, use the last path segment as `articleNumber`
3. Else, look up `Pages` table by `UrlPath` matching the Referer absolute path

Returns `null` if none match.

Notes:

- This method is designed primarily for editor iframe contexts where requests originate from the editing surface.
- External callers should ensure that a meaningful Referer is present or provide an alternative resolution mechanism in derived actions.

---

## Patterns & Best Practices

- Prefer calling these endpoints from within the CMS UI (editor/publisher) to ensure context (Referer, auth, antiforgery) is correct
- Respect rate limiting for contact and health endpoints
- When adding new base endpoints, follow the existing validation pattern:
  - `ModelState.IsValid` checks
  - Clear `BadRequest/NotFound` semantics
  - Centralize domain logic in `ArticleLogic` and services
- For CORS, mirror `GetTOC` if you need public cross-origin access and configure `AllCors`

## Extending in Derived Controllers

Example of inheritance (Publisher):

```csharp
public class HomeController : HomeControllerBase
{
    public HomeController(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger<HomeController> logger,
        ArticleLogic articleLogic,
        IOptions<CosmosConfig> options,
        ApplicationDbContext dbContext,
        StorageContext storageContext,
        IEmailSender emailSender)
        : base(articleLogic, dbContext, storageContext, logger, emailSender)
    {
        // Derived-only dependencies and behavior
    }
}
```

## Related Components

- `ArticleLogic` — TOC, search, and article queries
- `CosmosUtilities` — Storage utilities (folder listing, auth helpers)
- `ContactManagementService` — Contact submissions and email notifications
- `ApplicationDbContext` — EF Core models: `Pages`, etc.

---

Last updated: October 2025
