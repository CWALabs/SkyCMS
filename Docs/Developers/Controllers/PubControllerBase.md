# PubControllerBase Developer Documentation

Location: `Common/PubControllerBase.cs`

The `PubControllerBase` class provides a secure proxy for serving files from storage in the Publisher application. It centralizes access control for article-bound assets and ensures consistent streaming and content-type handling. Derived controllers can override behavior as needed.

- Namespace: `Cosmos.Common`
- Inherits: `Microsoft.AspNetCore.Mvc.Controller`
- Primary consumer: SkyCMS Publisher (`Sky.Publisher`)

---

## Constructor and Dependencies

```csharp
public PubControllerBase(
    ApplicationDbContext dbContext,
    StorageContext storageContext,
    bool requiresAuthentication)
```

- `ApplicationDbContext` — EF Core context used to verify page/article authorization
- `StorageContext` — Abstraction for blob/file storage; used to resolve file properties and streams
- `requiresAuthentication` — Global flag indicating whether Publisher requires authenticated access for files

Notes:

- The `requiresAuthentication` flag controls both authentication enforcement and per-article authorization checks.

---

## Endpoint

### Index (virtual)

Serves files from storage, optionally enforcing authentication and authorization.

```csharp
public virtual Task<IActionResult> Index()
```

- HTTP Verb: GET
- Route: Determined by the derived controller (typically `/Pub/Index` or mapped as a catch-all route)
- Auth behavior:
  - If `requiresAuthentication` is true and the user is not authenticated, returns `401 Unauthorized`
  - If the request path targets an article asset under `/pub/articles/{articleNumber}/...`, verifies access via `CosmosUtilities.AuthUser(dbContext, User, articleNumber)`; if unauthorized, returns `401 Unauthorized`
- Caching:
  - When `requiresAuthentication` is true and auth checks pass, sets `Expires` to current UTC time (effectively discouraging caching)
- Storage lookup and streaming:
  - Calls `storageContext.GetFileAsync(Request.Path)` to get file properties (content type, last modified, name)
  - Calls `storageContext.GetStreamAsync(Request.Path)` to stream the file
  - Uses `properties.ContentType` if present, otherwise resolves via `Utilities.GetContentType(properties.Name)`
  - Returns `File(stream, contentType, lastModified: properties.ModifiedUtc, entityTag: null)`
- Error handling:
  - On storage errors or missing files, returns `404 Not Found`

Status codes:

- `200 OK` — File streamed successfully
- `401 Unauthorized` — Not authenticated or not authorized for `/pub/articles/{articleNumber}`
- `404 Not Found` — File not found or storage error

Example (browser request):

```text
GET /pub/articles/123/images/header.webp HTTP/1.1
Host: example.com
```

Example (curl):

```bash
curl -i https://example.com/pub/articles/123/images/header.webp
```

---

## Security Model

- Global authentication: Controlled by `requiresAuthentication`
- Per-article authorization: Triggered only for paths under `/pub/articles/`, using `CosmosUtilities.AuthUser` to validate that the current `User` is permitted to access the specified `articleNumber`
- Non-article assets (paths outside `/pub/articles/`) bypass per-article checks but still require authentication when `requiresAuthentication` is true

Notes:

- Ensure your derived controller's routing does not expose unintended paths without checks when `requiresAuthentication` is true
- If you introduce additional protected roots (e.g., `/pub/private/`), mirror the `/pub/articles/` authorization pattern

---

## Extensibility

- The method is marked `virtual`; derived controllers can override `Index()` to add:
  - Additional path-based authorization
  - Custom cache headers or ETag handling
  - Range requests/partial content support
  - Download naming (via `File(stream, contentType, fileDownloadName: ...)`)

Example override:

```csharp
public override async Task<IActionResult> Index()
{
    // Custom pre-checks
    var result = await base.Index();
    // Optionally inspect/modify the result (e.g., add headers)
    return result;
}
```

### Partial content (HTTP Range) example

To support media seeking and resumable downloads, you can implement HTTP Range handling in a derived controller. The basic flow:

1. Read the `Range` request header and parse the requested byte range
2. Resolve the file size from storage properties
3. Seek the storage stream to the start offset and limit the length
4. Set headers `Accept-Ranges: bytes` and `Content-Range: bytes {start}-{end}/{length}`
5. Return status `206 Partial Content`

Sketch (pseudo-code):

```csharp
public override async Task<IActionResult> Index()
{
  var properties = await storageContext.GetFileAsync(HttpContext.Request.Path);
  var stream = await storageContext.GetStreamAsync(HttpContext.Request.Path);
  var totalLength = properties.ContentLength; // custom metadata or infer
  Response.Headers["Accept-Ranges"] = "bytes";

  if (Request.Headers.TryGetValue("Range", out var rangeHeader))
  {
    // Parse header: e.g., "bytes=START-END"
    var (start, end) = ParseRange(rangeHeader, totalLength); // implement parser
    var length = (end - start) + 1;
    stream.Seek(start, SeekOrigin.Begin);
    Response.StatusCode = StatusCodes.Status206PartialContent;
    Response.Headers["Content-Range"] = $"bytes {start}-{end}/{totalLength}";
    return File(stream, properties.ContentType ?? Utilities.GetContentType(properties.Name));
  }

  // Fallback to base behavior
  return await base.Index();
}
```

Notes:

- Ensure `ContentLength` (or equivalent) is available from storage properties to calculate ranges
- For large files, consider serving via CDN or a storage service that natively supports range requests
- If your `StorageContext` supports ranged reads, prefer using that instead of `Seek`

---

## Related Components

- `StorageContext` — Storage operations: `GetFileAsync`, `GetStreamAsync`
- `CosmosUtilities.AuthUser` — Authorization helper for article-bound assets
- `ApplicationDbContext` — Used by `AuthUser` to validate group/role/page permissions
- `Utilities.GetContentType` — Fallback MIME type resolution by file name/extension

---

## Operational Considerations

- If Publisher is public (anonymous access), set `requiresAuthentication` to `false`
- For authenticated sites, consider stricter cache headers (e.g., `no-store`) in derived controllers
- To support large media files and seek operations, add HTTP Range handling in an override
- Monitor `404` rates to catch storage path mismatches or deployment issues

---

Last updated: November 2025 (.NET 9.0 update)

