# SkyCMS.Tests — Test Suite Guide (October 2025)

This guide documents the SkyCMS test suite structure, what’s covered, how to run the tests, and how to extend them. The project uses MSTest on .NET 9 with EF Core InMemory for fast, isolated runs.

- Project: `Sky.Tests.csproj`
- Framework: MSTest
- Target Framework: net9.0
- Packages: Microsoft.NET.Test.Sdk, MSTest, EFCore.InMemory, Moq, UserSecrets

---

## What’s covered

### 1) ArticleEditLogic — core flows
Files:
- `ArticleEditLogicTests.cs`
- `ArticleEditLogicExtendedTests.cs`
- `ArticleEditLogicAdditionalTests.cs`
- `ArticleEditLogicRemainingTests.cs`

Highlights:
- Create/edit/publish flows (article numbers, slugs, SaveArticle, publish date handling)
- Title validation (duplicates, reserved paths, case-insensitivity, retaining same title)
- Redirect generation on title change (DB redirect entity + static redirect artifact)
- Catalog entry creation/consistency
- Home page switching (CreateHomePage) and static page generation
- Unpublish and delete flows (mark deleted, remove pages, clear catalog)
- Static site behaviors (no-op when disabled, artifact existence checks)
- URL normalization and editable content markers

### 2) PublishingService
File:
- `PublishingServiceTests.cs`

Highlights:
- Publish sets dates and unpublishes earlier versions
- Page replacement (non-redirect removal; redirects preserved)
- ParentUrlPath derivation
- Static file creation when enabled
- Unpublish removes pages but preserves redirects

### 3) ReservedPaths service
File:
- `ReservedPathsTests.cs`

Highlights:
- Default system paths seeding and persistence
- Case-insensitive matching, nested paths, and CRUD for custom paths
- System-path invariants (cannot modify/remove)
- Consistency after add/remove cycles and metadata requirements

### 4) BlogController (Editor)
File:
- `BlogControllerTests.cs`

Highlights:
- Create/Edit/Delete blog streams (key generation, collision handling, default blog rules)
- Entries listing and ordering by published date
- Create/Edit blog entries (articles tied to a blog key) and publish controls
- Generic blog page preview and API (`GetBlogs`)

---

## Test infrastructure

- Base class: `ArticleEditLogicTestBase` centralizes DI setup, EF Core InMemory context, and helpers
- Each test uses a fresh in-memory database (per test or per class) to avoid cross-test state
- A minimal default layout is seeded where required by logic that expects it
- Storage operations use an in-memory/dummy `StorageContext` backing used by tests; no external I/O
- Lightweight fakes (e.g., view rendering) are used where needed

Example EF InMemory setup pattern:

```csharp
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString())
    .Options;
var db = new ApplicationDbContext(options);
```

---

## Running tests

From the solution root or from the `Tests` folder:

```powershell
# Run all tests
 dotnet test

# Run tests in this project only
 dotnet test .\Tests\Sky.Tests.csproj

# Filter: run a specific class (MSTest)
 dotnet test .\Tests\Sky.Tests.csproj --filter "FullyQualifiedName~BlogControllerTests"

# Filter: run a single test method
 dotnet test .\Tests\Sky.Tests.csproj --filter "FullyQualifiedName~ArticleEditLogicTests.CreateArticle_AssignsArticleNumberStartingAt1"
```

Notes:
- Use VS Test Explorer for a GUI experience
- Parallelization is disabled for many fixtures via `[DoNotParallelize]` due to shared in-memory resources

---

## Adding or updating tests

1. Create a new `[TestClass]` file alongside existing tests, or add `[TestMethod]` to an existing class
2. Use the `ArticleEditLogicTestBase` initialization pattern in `[TestInitialize]`
3. Prefer arranging data via public services (e.g., `ArticleEditLogic.CreateArticle`) rather than inserting entities directly, unless the test targets persistence details
4. Keep tests isolated and deterministic (explicit timestamps, no real network or storage)
5. If you add new dependencies to core services, extend the base test setup with minimal fakes/stubs

---

## Troubleshooting

| Problem | Likely cause | Fix |
|---------|--------------|-----|
| Failing “publish” tests | Missing layout/article prerequisites | Ensure layout seeding and root creation precede publish flows |
| Storage driver/stream errors | Misconfigured dummy storage | Use an Azure-style connection string pattern in test setup; avoid real endpoints |
| Conflicting test state | Shared in-memory DB across tests | Use a unique DB name per test or dispose per test class as done here |
| Slow test runs | Excessive setup in each test | Move common setup into the base class; reuse seeded data carefully |

---

## Coverage and quality

- This project does not include a coverage collector by default. If you want coverage:
  - Add `coverlet.collector` package to the test project
  - Then run with: `dotnet test /p:CollectCoverage=true`
- Consider adding tests for: sitemap generation, TOC JSON when static enabled, and advanced redirect chains

---

## Conventions

- MSTest attributes: `[TestClass]`, `[TestMethod]`, `[TestInitialize]`, `[TestCleanup]`
- Use `[DoNotParallelize]` when tests rely on shared singletons or global state
- Keep names descriptive: `MethodUnderTest_Condition_ExpectedBehavior`

---

Last updated: October 2025
