# ArticleEditLogicTests Setup Guide

This guide explains how the `ArticleEditLogicTests` in the `Sky.Tests` project are configured and how to extend them.

## 1. Purpose
The tests validate core behaviors of `ArticleEditLogic`:
- Creating an article (`CreateArticle_AssignsArticleNumberStartingAt1`)
- Duplicate title detection (`ValidateTitle_ReturnsFalse_ForDuplicateTitle`)
- Saving article updates (`SaveArticle_UpdatesTitleAndContent`)
- Publishing an article (`PublishArticle_SetsPublishedDate`)

## 2. Project References & Packages
`Sky.Tests.csproj` includes:
- Project reference to `Sky.Editor` (brings in all dependent projects transitively)
- NuGet packages:
  - `Microsoft.NET.Test.Sdk`
  - `MSTest` (meta package for MSTest framework)
  - `Microsoft.EntityFrameworkCore.InMemory` (isolated in?memory database for each test)

If adding new tests that need additional providers or tooling, update `Sky.Tests.csproj` accordingly.

## 3. Test Infrastructure
Each test run creates a fresh EF Core in?memory database to avoid cross?test state:
```csharp
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString())
    .Options;
_db = new ApplicationDbContext(options);
```

A minimal default layout is seeded because some article logic assumes at least one layout exists.

## 4. Dependencies Faked / Simplified
| Dependency | Strategy | Notes |
|------------|----------|-------|
| `IViewRenderService` | Lightweight fake returning static HTML | Only needed for publish/static page paths; logic under test does not parse output. |
| `StorageContext` | Constructed with a dummy Azure?style connection string | Avoids driver selection exceptions. No real I/O performed in current tests. |
| `EditorSettings` | Built from an in?memory `IConfiguration` + `HttpContextAccessor` | Supplies required URLs & flags.
| `IMemoryCache` | Real `MemoryCache` instance | Sufficient for caching reserved paths / author info.
| Logging | `NullLogger<ArticleEditLogic>` | Suppresses noise.

## 5. Running the Tests
From the solution root:
```bash
dotnet test
```
Or inside the `Tests` directory:
```bash
dotnet test Sky.Tests.csproj
```

## 6. Adding New Tests
1. Create a new `[TestMethod]` in `ArticleEditLogicTests.cs` (or a new class with `[TestClass]`).
2. Use the existing `Setup` method pattern to construct any additional seed data.
3. Prefer arranging data via public methods (`CreateArticle`) rather than inserting entities directly, unless testing low?level persistence edge cases.
4. If you need to inspect internal state not exposed publicly, consider extending coverage with higher level behaviors rather than using `InternalsVisibleTo` (keep encapsulation intact).

## 7. Common Extensions
| Scenario | Suggested Approach |
|----------|--------------------|
| Testing redirects after title change | Create article ? publish ? change title ? call `SaveArticle` ? assert new redirect in `Pages` with `StatusCode == Redirect`. |
| Testing unpublish | Publish article ? call `UnpublishArticle` ? assert `Pages` entry removed. |
| Testing catalog entry intro extraction | Create article with first `<p>` text ? publish ? fetch catalog entry and assert `Introduction`. |

## 8. Troubleshooting
| Issue | Cause | Fix |
|-------|-------|-----|
| Null `EditorSettings` values | Missing configuration keys | Ensure `CosmosPublisherUrl` and `AzureBlobStorageEndPoint` keys added in configuration builder. |
| Storage driver exception | Invalid connection string pattern | Use an Azure style string: `DefaultEndpointsProtocol=...;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net`. |
| Failing publish tests | Missing layout/article prerequisites | Confirm layout seeding occurs before publish. |

## 9. Code Coverage Suggestions
Consider adding tests for:
- `RestoreArticle` (trash ? active)
- Reserved path collision (`ValidateTitle` with paths from `GetReservedPaths`)
- Title change redirect logic (`SaveTitleChange` indirectly via `SaveArticle`)
- Static TOC generation (enable `StaticWebPages` in settings and verify blob operation is invoked — may require a storage mock wrapper if behavior becomes more complex).

## 10. Design Principles Followed
- **Isolation**: Each test gets a clean DB instance.
- **Fast Execution**: No real external services (storage, rendering, CDN) invoked.
- **Public API Focus**: Tests interact through `ArticleEditLogic` public methods only.
- **Determinism**: Avoid time?sensitive flakiness; explicit timestamps only where needed.

## 11. Updating for Future Changes
If `ArticleEditLogic` gains constructor dependencies:
- Add minimal fakes/stubs mirroring existing pattern.
- Keep setup logic centralized in `[TestInitialize]` to reduce duplication.

---
Maintainer Tip: If test startup time increases, consider reusing a single in?memory database per test class and manually clearing tables, but only if test ordering is not required and isolation can be guaranteed.
