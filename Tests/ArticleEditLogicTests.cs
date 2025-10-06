using Microsoft.EntityFrameworkCore;

namespace Sky.Tests;

[TestClass]
public class ArticleEditLogicTests : ArticleEditLogicTestBase
{
    [TestInitialize]
    public void Setup() => InitializeTestContext(seedLayout: true);


    [TestCleanup]
    public void Cleanup() => Db.Dispose();

    [TestMethod]
    public async Task CreateArticle_AssignsArticleNumberStartingAt1()
    {
        var vm = await Logic.CreateArticle("Acme Tools", TestUserId);
        Assert.AreEqual(1, vm.ArticleNumber);
        Assert.AreEqual("root", vm.UrlPath);
    }

    [TestMethod]
    public async Task ValidateTitle_ReturnsFalse_ForDuplicateTitle()
    {
        _ = await Logic.CreateArticle("Duplicate", TestUserId);
        var valid = await Logic.ValidateTitle("Duplicate", null);
        Assert.IsFalse(valid, "Expected duplicate title to be invalid");
    }

    [TestMethod]
    public async Task SaveArticle_UpdatesTitleAndContent()
    {
        var vm = await Logic.CreateArticle("Original", TestUserId);
        vm.Title = "Original Updated";
        vm.Content = "<div contenteditable='true' data-ccms-ceid='x'>Updated</div>";
        var result = await Logic.SaveArticle(vm, TestUserId);
        Assert.IsTrue(result.ServerSideSuccess);
        var reloaded = await Logic.GetArticleByArticleNumber(vm.ArticleNumber, null);
        Assert.AreEqual("Original Updated", reloaded.Title);
    }

    [TestMethod]
    public async Task PublishArticle_SetsPublishedDate()
    {
        var vm = await Logic.CreateArticle("Publish Me", TestUserId);
        var entity = await Db.Articles.FirstAsync(a => a.ArticleNumber == vm.ArticleNumber);
        await Logic.PublishArticle(entity.Id, DateTimeOffset.UtcNow);
        var published = await Logic.GetLastPublishedDate(vm.ArticleNumber);
        Assert.IsNotNull(published);
    }
}
