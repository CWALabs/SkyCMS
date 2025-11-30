using Microsoft.EntityFrameworkCore;

namespace Sky.Tests.Services
{
    [TestClass]
    public class TitleChangeServiceTests : SkyCmsTestBase
    {
        [TestInitialize]
        public new void Setup() => InitializeTestContext();

        [TestMethod]
        public async Task HandleTitleChangeAsync_RootPage_PreservesRootUrlPath()
        {
            // Arrange
            var rootArticle = await Logic.CreateArticle("Home Page", TestUserId);
            var article = await Db.Articles.FirstAsync(a => a.ArticleNumber == rootArticle.ArticleNumber);
            
            Assert.AreEqual("root", article.UrlPath);

            // Capture the old URL path BEFORE changing the title
            var oldUrlPath = article.UrlPath; // "root"
            var oldTitle = article.Title;

            article.Title = "New Home Page";

            // Act - Pass the old URL path (not the old title)
            await TitleChangeService.HandleTitleChangeAsync(article, oldTitle, oldUrlPath);
            await Db.SaveChangesAsync();

            // Assert
            var updated = await Db.Articles.FirstAsync(a => a.Id == article.Id);
            Assert.AreEqual("root", updated.UrlPath);
            Assert.AreEqual("New Home Page", updated.Title);
        }

        [TestMethod]
        public async Task HandleTitleChangeAsync_NonRootPage_ChangesUrlPath()
        {
            // Arrange
            await Logic.CreateArticle("Home Page", TestUserId); // Create root first
            var nonRootArticle = await Logic.CreateArticle("About Us", TestUserId);
            var article = await Db.Articles.FirstAsync(a => a.ArticleNumber == nonRootArticle.ArticleNumber);
            
            var originalPath = article.UrlPath;
            var oldTitle = article.Title;
            Assert.AreNotEqual("root", originalPath);
            Assert.AreEqual("about-us", originalPath);

            // Capture the old URL path BEFORE changing the title
            var oldUrlPath = article.UrlPath; // "about-us"
            article.Title = "Company Information"; // Update to new title

            // Act - Pass the old URL path (not the old title)
            await TitleChangeService.HandleTitleChangeAsync(article, oldTitle, oldUrlPath);
            await Db.SaveChangesAsync();

            // Assert
            var updated = await Db.Articles.FirstAsync(a => a.Id == article.Id);
            Assert.AreNotEqual(originalPath, updated.UrlPath);
            Assert.AreEqual("company-information", updated.UrlPath);
        }

        [TestMethod]
        public async Task HandleTitleChangeAsync_RootPageWithChildren_ChildrenUnaffected()
        {
            // Arrange
            var rootArticle = await Logic.CreateArticle("Home Page", TestUserId);
            var childArticle = await Logic.CreateArticle("root/child", TestUserId);
            
            var root = await Db.Articles.FirstAsync(a => a.ArticleNumber == rootArticle.ArticleNumber);
            var child = await Db.Articles.FirstAsync(a => a.ArticleNumber == childArticle.ArticleNumber);

            // **FIX**: Store old title and update to new title
            var oldTitle = root.Title;
            var oldUrlPath = root.UrlPath;

            root.Title = "Main Page";

            // Act - Change root title
            await TitleChangeService.HandleTitleChangeAsync(root, oldTitle, oldUrlPath);
            await Db.SaveChangesAsync();

            // Assert
            var updatedRoot = await Db.Articles.FirstAsync(a => a.Id == root.Id);
            var updatedChild = await Db.Articles.FirstAsync(a => a.Id == child.Id);
            
            Assert.AreEqual("root", updatedRoot.UrlPath);
            // Child should not be affected since root UrlPath didn't change
            Assert.AreEqual("root/child", updatedChild.UrlPath);
        }

        [TestMethod]
        public async Task BuildArticleUrl_RootPage_ReturnsNormalizedSlug()
        {
            // Arrange
            var rootArticle = await Logic.CreateArticle("Home Page", TestUserId);
            var article = await Db.Articles.FirstAsync(a => a.ArticleNumber == rootArticle.ArticleNumber);

            // Act
            var url = TitleChangeService.BuildArticleUrl(article);

            // Assert
            // BuildArticleUrl doesn't know about root - it just normalizes
            // The HandleTitleChangeAsync method is responsible for preserving "root"
            Assert.AreEqual("home-page", url);
        }
    }
}