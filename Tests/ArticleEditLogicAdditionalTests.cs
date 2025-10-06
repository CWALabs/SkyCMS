using Microsoft.EntityFrameworkCore;

namespace Sky.Tests;

[TestClass]
public class ArticleEditLogicAdditionalTests : ArticleEditLogicTestBase
{
    [TestInitialize]
    public void Setup() => InitializeTestContext();

    [TestCleanup]
    public void Cleanup() => Db.Dispose();

    [TestMethod]
    public async Task UnpublishArticle_RemovesPublishedPages()
    {
        await Logic.CreateArticle("Home Page", TestUserId);
        var art = await Logic.CreateArticle("Temp Page", TestUserId);
        var ent = await Db.Articles.FirstAsync(a => a.ArticleNumber == art.ArticleNumber);
        await Logic.PublishArticle(ent.Id, DateTimeOffset.UtcNow);

        Assert.IsTrue(await Db.Pages.AnyAsync(p => p.ArticleNumber == art.ArticleNumber));

        await Logic.UnpublishArticle(art.ArticleNumber);

        Assert.IsFalse(await Db.Pages.AnyAsync(p => p.ArticleNumber == art.ArticleNumber));
        var versions = await Db.Articles.Where(a => a.ArticleNumber == art.ArticleNumber).ToListAsync();
        Assert.IsTrue(versions.All(v => v.Published == null));
    }

    [TestMethod]
    public async Task ValidateTitle_DifferentArticleSameTitle_ReturnsFalse()
    {
        await Logic.CreateArticle("Home Page", TestUserId);
        var a1 = await Logic.CreateArticle("Duplicate Title", TestUserId);
        var a2 = await Logic.CreateArticle("Another Page", TestUserId);

        var valid = await Logic.ValidateTitle("Duplicate Title", a2.ArticleNumber);
        Assert.IsFalse(valid);
    }

    [TestMethod]
    public async Task GetLastPublishedDate_ReturnsMostRecent()
    {
        var root = await Logic.CreateArticle("Home Page", TestUserId); // published
        var page = await Logic.CreateArticle("Publish Test", TestUserId);
        var e1 = await Db.Articles.FirstAsync(a => a.ArticleNumber == page.ArticleNumber);

        await Logic.PublishArticle(e1.Id, DateTimeOffset.UtcNow.AddMinutes(-10));

        // new version & publish
        var vm = await Logic.GetArticleByArticleNumber(page.ArticleNumber, null);
        vm.Content += " updated";
        await Logic.SaveArticle(vm, TestUserId);
        var latest = await Db.Articles
            .Where(a => a.ArticleNumber == page.ArticleNumber)
            .OrderByDescending(a => a.VersionNumber)
            .FirstAsync();
        var now = DateTimeOffset.UtcNow;
        await Logic.PublishArticle(latest.Id, now);

        var last = await Logic.GetLastPublishedDate(page.ArticleNumber);
        Assert.IsTrue(last.HasValue);
        Assert.IsTrue((now - last.Value).Duration() < TimeSpan.FromSeconds(2));
    }

    [TestMethod]
    public async Task SaveArticle_TitleChange_UpdatesChildUrls()
    {
        await Logic.CreateArticle("Home Page", TestUserId);
        var parent = await Logic.CreateArticle("Parent Section", TestUserId);
        var child = await Logic.CreateArticle("Parent Section Child Doc", TestUserId);

        // rename parent
        var parentVm = await Logic.GetArticleByArticleNumber(parent.ArticleNumber, null);
        parentVm.Title = "Parent Renamed";
        await Logic.SaveArticle(parentVm, TestUserId);

        var childVm = await Logic.GetArticleByArticleNumber(child.ArticleNumber, null);
        Assert.IsTrue(childVm.UrlPath.StartsWith("parent_renamed", StringComparison.OrdinalIgnoreCase),
            "Expected child URL updated with new parent slug.");
    }
}