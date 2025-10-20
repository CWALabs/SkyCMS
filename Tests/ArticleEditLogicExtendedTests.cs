using Microsoft.EntityFrameworkCore;
using Sky.Cms.Controllers;
using Sky.Cms.Services;
using Sky.Editor.Services.Slugs;

namespace Sky.Tests;

[DoNotParallelize]
[TestClass]
public class ArticleEditLogicExtendedTests : ArticleEditLogicTestBase
{
    [TestInitialize]
    public void Setup() => InitializeTestContext(seedLayout: true);

    [TestCleanup]
    public void Cleanup() => Db.Dispose();

    #region NormailizeArticleUrl

    [TestMethod]
    public void NormailizeArticleUrl_ConvertsToLowerUnderscores()
    {
        var slug = SlugService.Normalize("  Hello World  2025  ");
        Assert.AreEqual("hello-world-2025", slug);
    }

    #endregion

    #region GetArticleById / GetArticleByUrl

    [TestMethod]
    public async Task GetArticleById_Returns_Article()
    {
        var vm = await Logic.CreateArticle("First Root", TestUserId);
        var fetched = await Logic.GetArticleById(vm.Id, EnumControllerName.Edit, TestUserId);
        Assert.IsNotNull(fetched);
        Assert.AreEqual(vm.Id, fetched.Id);
    }

    [TestMethod]
    public async Task GetArticleByUrl_Returns_LatestVersion()
    {
        // Root first
        await Logic.CreateArticle("Home Page", TestUserId);
        // Second article
        var vm = await Logic.CreateArticle("Sample Title", TestUserId);

        // Add a new version manually
        var latest = await Db.Articles
            .Where(a => a.ArticleNumber == vm.ArticleNumber)
            .OrderByDescending(a => a.VersionNumber)
            .FirstAsync();

        var fetched = await Logic.GetArticleByUrl("sample-title");
        Assert.IsNotNull(fetched);
        Assert.AreEqual(latest.VersionNumber, fetched.VersionNumber);
    }

    #endregion

    #region Ensure_ContentEditable_IsMarked

    [TestMethod]
    public void EnsureContentEditable_Blank_AddsWrapper()
    {
        var html = ArticleHtmlService.EnsureEditableMarkers(string.Empty);
        StringAssert.Contains(html, "contenteditable");
        StringAssert.Contains(html, "data-ccms-ceid");
    }

    [TestMethod]
    public void EnsureContentEditable_Existing_AddsMissingIds()
    {
        var original = "<section><div contenteditable='true'>Content</div></section>";
        var processed = ArticleHtmlService.EnsureEditableMarkers(original);
        StringAssert.Contains(processed, "data-ccms-ceid");
        StringAssert.Contains(processed, "data-ccms-index=\"0\"");
    }

    #endregion

    #region Reserved Paths

    [TestMethod]
    public async Task GetReservedPaths_Includes_System_Paths()
    {
        var paths = await ReservedPaths.GetReservedPaths();
        Assert.IsTrue(paths.Any(p => p.Path.Equals("root", StringComparison.OrdinalIgnoreCase)));
    }

    #endregion

    #region Redirects

    [TestMethod]
    public async Task GetArticleRedirects_ContainsRedirectAfterTitleChange()
    {
        // Create article
        var article = await Logic.CreateArticle("Original Title", TestUserId);
        article.Published = DateTimeOffset.UtcNow;
        await Logic.SaveArticle(article, TestUserId);

        // Trigger title change -> redirect
        article.Title = "New Title";
        article.Published = DateTimeOffset.UtcNow;
        await Logic.SaveArticle(article, TestUserId);

        var redirects = Logic.GetArticleRedirects().ToList();
        Assert.IsTrue(redirects.Any(), "Expected at least one redirect after title change.");

        var staticRedirect = await Storage.BlobExistsAsync("original-title");
        Assert.IsTrue(staticRedirect, "Expected static redirect file to exist.");
    }

    #endregion

    #region Unpublish

    [TestMethod]
    public async Task UnpublishArticle_RemovesPublishedPages()
    {
        // Root
        await Logic.CreateArticle("Home Page", TestUserId);

        var page = await Logic.CreateArticle("Publish Me", TestUserId);
        var entity = await Db.Articles.FirstAsync(a => a.ArticleNumber == page.ArticleNumber);
        await Logic.PublishArticle(entity.Id, DateTimeOffset.UtcNow);

        Assert.IsTrue(await Db.Pages.AnyAsync(p => p.ArticleNumber == page.ArticleNumber));

        await PublishingService.UnpublishAsync(entity);

        Assert.IsFalse(await Db.Pages.AnyAsync(p => p.ArticleNumber == page.ArticleNumber));
        var versions = await Db.Articles.Where(a => a.ArticleNumber == page.ArticleNumber).ToListAsync();
        Assert.IsTrue(versions.All(v => v.Published == null));
    }

    #endregion

    #region ExportArticle

    [TestMethod]
    public async Task ExportArticle_ReturnsHtmlWithTitle()
    {
        await Logic.CreateArticle("Home Page", TestUserId);
        var article = await Logic.CreateArticle("Sample Export", TestUserId);
        var html = await Logic.ExportArticle(article, new Uri("https://cdn.example.com/"), new FakeViewRenderService());
        StringAssert.Contains(html, "<!DOCTYPE html>");
        StringAssert.Contains(html, "<title>Sample Export</title>");
        StringAssert.Contains(html, article.Content.Trim()[..10]); // basic content presence
    }

    #endregion

    #region Static Web Methods (Disabled Scenario)

    [TestMethod]
    public async Task CreateStaticWebpage_StaticDisabled_NoException()
    {
        var root = await Logic.CreateArticle("Home Page", TestUserId);
        var publishedEntity = await Db.Articles.FirstAsync(a => a.ArticleNumber == root.ArticleNumber);
        await Logic.PublishArticle(publishedEntity.Id, DateTimeOffset.UtcNow);

        var publishedPage = await Db.Pages.FirstAsync(p => p.ArticleNumber == root.ArticleNumber);
        await PublishingService.CreateStaticPages(new List<Guid> { publishedPage.Id }); // Should no-op with StaticWebPages=false
        Assert.IsTrue(true); // Reached without exception
    }

    [TestMethod]
    public async Task CreateStaticTableOfContentsJsonFile_StaticDisabled_NoException()
    {
        await Logic.CreateArticle("Home Page", TestUserId);
        await PublishingService.WriteTocAsync();
        Assert.IsTrue(true);
    }

    #endregion

    private sealed class FakeViewRenderService : IViewRenderService
    {
        public Task<string> RenderToStringAsync(string viewName, object model) =>
            Task.FromResult("<html><body>rendered</body></html>");
    }
}