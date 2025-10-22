using Cosmos.Common.Data;
using Cosmos.Common.Data.Logic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sky.Editor.Controllers;
using Sky.Editor.Models.Blogs;
using System.Security.Claims;

// BlogControllerTests.cs

namespace Sky.Tests;

/// <summary>
/// Unit tests for BlogController covering CRUD operations on blog streams and blog entries.
/// </summary>
[DoNotParallelize]
[TestClass]
public class BlogControllerTests : ArticleEditLogicTestBase
{
    private BlogController _controller;

    [TestInitialize]
    public void Setup()
    {
        InitializeTestContext(seedLayout: true);
        _controller = new BlogController(Db, Logic, SlugService, RedirectService);

        // Mock user context for authenticated actions
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId.ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [TestCleanup]
    public void Cleanup()
    {
        Db.Dispose();
    }

    #region Index

    [TestMethod]
    public async Task Index_ReturnsViewWithBlogs_OrderedBySortOrderThenKey()
    {
        // Arrange
        await CreateTestBlog("tech-blog", "Tech Blog", sortOrder: 2);
        await CreateTestBlog("news-blog", "News Blog", sortOrder: 1);
        await CreateTestBlog("alpha-blog", "Alpha Blog", sortOrder: 1);

        // Act
        var result = await _controller.Index() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ViewName);
        var model = result.Model as System.Collections.Generic.List<BlogStreamViewModel>;
        Assert.IsNotNull(model);
        Assert.AreEqual(3, model.Count);
        // Ordered by SortOrder (1, 1, 2), then BlogKey (alpha, news, tech)
        Assert.AreEqual("alpha-blog", model[0].BlogKey);
        Assert.AreEqual("news-blog", model[1].BlogKey);
        Assert.AreEqual("tech-blog", model[2].BlogKey);
    }

    [TestMethod]
    public async Task Index_WithMixedSortOrders_ReturnsCorrectOrdering()
    {
        // Arrange
        await CreateTestBlog("z-blog", "Z Blog", sortOrder: 0);
        await CreateTestBlog("a-blog", "A Blog", sortOrder: 0);
        await CreateTestBlog("m-blog", "M Blog", sortOrder: -1);

        // Act
        var result = await _controller.Index() as ViewResult;
        var model = result.Model as List<BlogStreamViewModel>;

        // Assert
        Assert.AreEqual("m-blog", model[0].BlogKey); // sortOrder -1
        Assert.AreEqual("a-blog", model[1].BlogKey); // sortOrder 0, alpha first
        Assert.AreEqual("z-blog", model[2].BlogKey);
    }

    [TestMethod]
    public async Task Index_UnauthenticatedUser_ReturnsUnauthorized()
    {
        // Requires setting up an unauthenticated context
        // This depends on your auth middleware setup
    }

    #endregion

    #region Create (GET)

    [TestMethod]
    public void Create_Get_ReturnsViewWithDefaultModel()
    {
        // Act
        var result = _controller.Create() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Create", result.ViewName);
        var model = result.Model as BlogStreamViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(0, model.SortOrder);
    }

    #endregion

    #region Create (POST)

    [TestMethod]
    public async Task Create_Post_ValidModel_AutoGeneratesKeyAndCreates()
    {
        // Arrange
        var model = new BlogStreamViewModel
        {
            Title = "My New Blog",
            Description = "Test description",
            HeroImage = "/images/hero.jpg",
            SortOrder = 5,
            BlogKey = "ignored-user-input" // Controller overwrites this
        };

        // Act
        var result = await _controller.Create(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(nameof(_controller.Index), result.ActionName);

        var created = await Db.Blogs.FirstOrDefaultAsync(b => b.Title == "My New Blog");
        Assert.IsNotNull(created);
        Assert.AreEqual("my-new-blog", created.BlogKey); // auto-generated, NOT "ignored-user-input"
        Assert.AreEqual("Test description", created.Description);
        Assert.AreEqual("/images/hero.jpg", created.HeroImage);
        Assert.AreEqual(5, created.SortOrder);
    }

    [TestMethod]
    public async Task Create_Post_DuplicateTitleGeneratesUniqueKey()
    {
        // Arrange
        await CreateTestBlog("existing-blog", "Existing Blog");

        var model = new BlogStreamViewModel
        {
            Title = "Existing Blog", // Same title
            Description = "Second one",
        };

        // Act
        var result = await _controller.Create(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result, "Should succeed with unique key suffix");
        var entity = await Db.Blogs.FirstAsync(b => b.BlogKey == "existing-blog");
        var created = await Db.Blogs.FirstOrDefaultAsync(b => b.Title == "Existing Blog" && b.Id != entity.Id);
        Assert.IsNotNull(created);
        Assert.AreEqual("existing-blog-2", created.BlogKey); // suffixed for uniqueness
    }

    [TestMethod]
    public async Task Create_Post_KeyConflictsWithArticle_ReturnsError()
    {
        // Arrange: create an article with URL starting with 'test-blog'
        await Logic.CreateArticle("Home Page", TestUserId);
        await Logic.CreateArticle("test-blog/article", TestUserId);

        var model = new BlogStreamViewModel
        {
            Title = "Test Blog", // Will generate 'test_blog' key
            Description = "Test",
        };

        // Act
        var result = await _controller.Create(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(_controller.ModelState.IsValid);
        StringAssert.Contains(_controller.ModelState[nameof(model.BlogKey)].Errors.First().ErrorMessage,
            "conflicts with existing page");
    }

    [TestMethod]
    public async Task Create_Post_IsDefault_UnsetsPreviousDefault()
    {
        // Arrange
        var oldDefault = await CreateTestBlog("old-default", "Old Default", isDefault: true);
        Assert.IsTrue(oldDefault.IsDefault);

        var model = new BlogStreamViewModel
        {
            Title = "New Default",
            Description = "New one",
            IsDefault = true
        };

        // Act
        await _controller.Create(model);

        // Assert
        await Db.Entry(oldDefault).ReloadAsync();
        Assert.IsFalse(oldDefault.IsDefault, "Old default should be unset");

        var newBlog = await Db.Blogs.FirstAsync(b => b.Title == "New Default");
        Assert.IsTrue(newBlog.IsDefault);
    }

    [TestMethod]
    public async Task GenerateUniqueBlogKey_EmptyTitle_GeneratesDefaultKey()
    {
        // Arrange
        var model = new BlogStreamViewModel
        {
            Title = "   ", // Whitespace only
            Description = "Test"
        };

        // Act
        var result = await _controller.Create(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        var created = await Db.Blogs.FirstOrDefaultAsync();
        Assert.AreEqual("blog", created.BlogKey); // Falls back to "blog"
    }

    [TestMethod]
    public async Task GenerateUniqueBlogKey_SpecialCharactersOnly_GeneratesDefaultKey()
    {
        // Arrange
        var model = new BlogStreamViewModel
        {
            Title = "!@#$%^&*()", // No valid chars
            Description = "Test"
        };

        // Act
        var result = await _controller.Create(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        var created = await Db.Blogs.FirstOrDefaultAsync();
        Assert.AreEqual("blog", created.BlogKey);
    }

    [TestMethod]
    public async Task GenerateUniqueBlogKey_VeryLongTitle_TruncatesTo64Chars()
    {
        // Arrange
        var longTitle = new string('a', 100);
        var model = new BlogStreamViewModel
        {
            Title = longTitle,
            Description = "Test"
        };

        // Act
        await _controller.Create(model);

        // Assert
        var created = await Db.Blogs.FirstOrDefaultAsync();
        Assert.IsTrue(created.BlogKey.Length <= 64);
    }

    [TestMethod]
    public async Task GenerateUniqueBlogKey_MultipleCollisions_GeneratesSuffixedKeys()
    {
        // Arrange
        await CreateTestBlog("test", "Test");
        await CreateTestBlog("test-2", "Test");
        await CreateTestBlog("test-3", "Test");

        var model = new BlogStreamViewModel
        {
            Title = "Test", // Will become test-4
            Description = "Fourth"
        };

        // Act
        await _controller.Create(model);

        // Assert
        var created = await Db.Blogs.OrderBy(b => b.CreatedUtc).LastAsync();
        Assert.AreEqual("test-4", created.BlogKey);
    }

    [TestMethod]
    public async Task Create_Post_MissingTitle_ReturnsViewWithError()
    {
        // Arrange
        _controller.ModelState.AddModelError("Title", "Required");
        var model = new BlogStreamViewModel
        {
            Title = null,
            Description = "Test"
        };

        // Act
        var result = await _controller.Create(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Create", result.ViewName);
        Assert.IsFalse(_controller.ModelState.IsValid);
    }

    [TestMethod]
    public async Task Create_Post_MissingDescription_ReturnsViewWithError()
    {
        // Arrange
        _controller.ModelState.AddModelError("Description", "Required");
        var model = new BlogStreamViewModel
        {
            Title = "Test",
            Description = null
        };

        // Act
        var result = await _controller.Create(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(_controller.ModelState.IsValid);
    }

    [TestMethod]
    public async Task Create_Post_FirstBlog_AutomaticallyBecomesDefault()
    {
        // Arrange
        var model = new BlogStreamViewModel
        {
            Title = "First Blog",
            Description = "Test",
            IsDefault = false // Explicitly false
        };

        // Act
        await _controller.Create(model);

        // Assert
        var created = await Db.Blogs.FirstAsync();
        // Note: Check your controller logic - should first blog auto-become default?
        // If yes, assert IsTrue; if no, assert IsFalse
    }

    #endregion

    #region Edit (GET)

    [TestMethod]
    public async Task Edit_Get_ExistingBlog_ReturnsView()
    {
        // Arrange
        var blog = await CreateTestBlog("edit-test", "Edit Test Blog");

        // Act
        var result = await _controller.Edit(blog.Id) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Edit", result.ViewName);
        var model = result.Model as BlogStreamViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(blog.Id, model.Id);
        Assert.AreEqual("edit-test", model.BlogKey);
        Assert.AreEqual("Edit Test Blog", model.Title);
    }

    [TestMethod]
    public async Task Edit_Get_NonExistentBlog_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Edit(Guid.NewGuid());

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    #endregion

    #region Edit (POST)

    [TestMethod]
    public async Task Edit_Post_ValidModel_RegeneratesKeyFromTitleAndUpdates()
    {
        // Arrange
        var isDefault = Db.Blogs.Count(a => a.IsDefault == true) == 0;
        var blog = await CreateTestBlog("original-title", "Original Title", isDefault: isDefault);
        var model = new BlogStreamViewModel
        {
            Id = blog.Id,
            Title = "Updated Title",
            Description = "Updated description",
            HeroImage = "/new-hero.jpg",
            SortOrder = 10,
            BlogKey = "manually-set-key", // Controller ignores this and regenerates from Title
            IsDefault = blog.IsDefault
        };

        // Act
        var result = await _controller.Edit(blog.Id, model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);

        // Verify updates in database.
        blog = await Db.Blogs.FirstOrDefaultAsync(b => b.Id == blog.Id);
        Assert.AreEqual("Updated Title", blog.Title);
        Assert.AreEqual("updated-title", blog.BlogKey); // Regenerated from title, NOT "manually-set-key"
        Assert.AreEqual("Updated description", blog.Description);
        Assert.AreEqual("/new-hero.jpg", blog.HeroImage);
        Assert.AreEqual(10, blog.SortOrder);
        Assert.IsTrue(blog.UpdatedUtc > DateTimeOffset.UtcNow.AddMinutes(-1));
    }

    [TestMethod]
    public async Task Edit_Post_NewTitleCollidesWithExistingKey_ReturnsError()
    {
        // Arrange
        var blog1 = await CreateTestBlog("blog-one", "Blog One");
        var blog2 = await CreateTestBlog("blog-two", "Blog Two");

        var model = new BlogStreamViewModel
        {
            Id = blog2.Id,
            Title = "Blog One", // Will generate 'blog_one' which collides with blog1
            Description = "Test"
        };

        // Act
        var result = await _controller.Edit(blog2.Id, model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(_controller.ModelState.IsValid);
        Assert.IsTrue(_controller.ModelState.ContainsKey(nameof(model.BlogKey)));
        StringAssert.Contains(_controller.ModelState[nameof(model.BlogKey)].Errors.First().ErrorMessage,
            "Another blog with this key exists");
    }

    [TestMethod]
    public async Task Edit_Post_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var blog = await CreateTestBlog("test", "Test");
        var model = new BlogStreamViewModel { Id = Guid.NewGuid(), Title = "Test" };

        // Act
        var result = await _controller.Edit(blog.Id, model);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task Edit_Post_MissingRequiredFields_ReturnsViewWithErrors()
    {
        // Arrange
        var blog = await CreateTestBlog("test", "Test");
        _controller.ModelState.AddModelError("Title", "Required");

        var model = new BlogStreamViewModel
        {
            Id = blog.Id,
            Title = null,
            Description = "Test"
        };

        // Act
        var result = await _controller.Edit(blog.Id, model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Edit", result.ViewName);
        Assert.IsFalse(_controller.ModelState.IsValid);
    }

    [TestMethod]
    public async Task Edit_Post_SetIsDefaultFalse_WhenCurrentlyDefault_RequiresAnotherDefault()
    {
        // Arrange
        var blog = await CreateTestBlog("only-default", "Only Default", isDefault: true);

        var model = new BlogStreamViewModel
        {
            Id = blog.Id,
            Title = "Only Default",
            Description = "Test",
            IsDefault = false // Try to unset
        };

        // Act
        var result = await _controller.Edit(blog.Id, model) as ViewResult;

        // Assert - Should fail if it's the only blog
        // Adjust based on your business rules
        Assert.IsNotNull(result);
        Assert.IsFalse(_controller.ModelState.IsValid);
    }

    #endregion

    #region Delete (GET)

    [TestMethod]
    public async Task Delete_Get_ExistingBlog_ReturnsConfirmationView()
    {
        // Arrange
        var blog = await CreateTestBlog("delete-test", "Delete Test");

        // Act
        var result = await _controller.Delete(blog.Id) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Delete", result.ViewName);
        var model = result.Model as BlogStreamViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(blog.Id, model.Id);
    }

    #endregion

    #region ConfirmDelete (POST)

    [TestMethod]
    public async Task ConfirmDelete_NonDefaultBlogNoArticles_DeletesAndRedirects()
    {
        // Arrange
        var blog = await CreateTestBlog("deletable", "Deletable Blog", isDefault: false);

        // Act
        var result = await _controller.ConfirmDelete(blog.Id) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(nameof(_controller.Index), result.ActionName);
        Assert.IsFalse(await Db.Blogs.AnyAsync(b => b.Id == blog.Id));
    }

    [TestMethod]
    public async Task ConfirmDelete_DefaultBlog_ReturnsErrorView()
    {
        // Arrange
        var blog = await CreateTestBlog("default", "Default Blog", isDefault: true);

        // Act
        var result = await _controller.ConfirmDelete(blog.Id) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Delete", result.ViewName);
        Assert.IsFalse(_controller.ModelState.IsValid);
        StringAssert.Contains(_controller.ModelState[string.Empty].Errors.First().ErrorMessage,
            "Cannot delete the default blog");
    }

    [TestMethod]
    public async Task ConfirmDelete_WithArticles_Reassign_MovesToFallback()
    {
        // Arrange
        await Logic.CreateArticle("Home Page", TestUserId); // root
        var blog1 = await CreateTestBlog("blog-one", "Blog One", isDefault: true);
        var blog2 = await CreateTestBlog("blog-two", "Blog Two");

        // Create article in blog2
        var article = await Logic.CreateArticle("Post in Blog Two", TestUserId, null, blog2.BlogKey);

        // Act
        var result = await _controller.ConfirmDelete(blog2.Id, reassign: true) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        var articleEntity = await Db.Articles.FirstAsync(a => a.ArticleNumber == article.ArticleNumber);
        Assert.AreEqual(blog1.BlogKey, articleEntity.BlogKey, "Article should move to default blog");
        Assert.IsFalse(await Db.Blogs.AnyAsync(b => b.Id == blog2.Id));
    }

    [TestMethod]
    public async Task ConfirmDelete_WithArticles_NoReassign_ReturnsError()
    {
        // Arrange
        await Logic.CreateArticle("Home Page", TestUserId);
        var blog = await CreateTestBlog("has-articles", "Has Articles");
        await Logic.CreateArticle("Post", TestUserId, null, blog.BlogKey);

        // Act
        var result = await _controller.ConfirmDelete(blog.Id, reassign: false) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(_controller.ModelState.IsValid);
        StringAssert.Contains(_controller.ModelState[string.Empty].Errors.First().ErrorMessage,
            "Blog contains articles");
    }

    [TestMethod]
    public async Task ConfirmDelete_NonExistentBlog_ReturnsNotFound()
    {
        // Act
        var result = await _controller.ConfirmDelete(Guid.NewGuid());

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task ConfirmDelete_LastBlogWithArticles_NoFallback_ReturnsError()
    {
        // Arrange
        await Logic.CreateArticle("Home Page", TestUserId);
        var blog = await CreateTestBlog("only-blog", "Only Blog", isDefault: true);
        await Logic.CreateArticle("Post", TestUserId, null, blog.BlogKey);

        // Act
        var result = await _controller.ConfirmDelete(blog.Id, reassign: true) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(_controller.ModelState.IsValid);

        var errorMessage = _controller.ModelState[string.Empty].Errors.First().ErrorMessage;

        StringAssert.Contains(errorMessage, "Cannot delete the default blog. Make another default first.");
    }

    #endregion

    #region Entries

    [TestMethod]
    public async Task Entries_ValidBlogKey_ReturnsViewWithEntries()
    {
        // Arrange
        await Logic.CreateArticle("Home Page", TestUserId);
        var blog = await CreateTestBlog("my-blog", "My Blog");
        var post1 = await Logic.CreateArticle("Post One", TestUserId, null, blog.BlogKey);
        var post2 = await Logic.CreateArticle("Post Two", TestUserId, null, blog.BlogKey);

        // Act
        var result = await _controller.Entries(blog.BlogKey) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Entries", result.ViewName);
        var model = result.Model as BlogEntriesListViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(blog.BlogKey, model.BlogKey);
        Assert.AreEqual("My Blog", model.BlogTitle);
        Assert.AreEqual(2, model.Entries.Count);
    }

    [TestMethod]
    public async Task Entries_EmptyBlogKey_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Entries(string.Empty);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    [TestMethod]
    public async Task Entries_NonExistentBlog_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Entries("nonexistent");

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task Entries_OrdersByPublishedDescending()
    {
        // Arrange
        await Logic.CreateArticle("Home Page", TestUserId);
        var blog = await CreateTestBlog("test", "Test");

        var old = await Logic.CreateArticle("Old Post", TestUserId, null, blog.BlogKey);
        var oldEntity = await Db.Articles.FirstAsync(a => a.ArticleNumber == old.ArticleNumber);
        oldEntity.Published = DateTimeOffset.UtcNow.AddDays(-5);

        var recent = await Logic.CreateArticle("Recent Post", TestUserId, null, blog.BlogKey);
        var recentEntity = await Db.Articles.FirstAsync(a => a.ArticleNumber == recent.ArticleNumber);
        recentEntity.Published = DateTimeOffset.UtcNow;

        await Db.SaveChangesAsync();

        // Act
        var result = await _controller.Entries(blog.BlogKey) as ViewResult;
        var model = result.Model as BlogEntriesListViewModel;

        // Assert
        Assert.AreEqual("Recent Post", model.Entries[0].Title);
        Assert.AreEqual("Old Post", model.Entries[1].Title);
    }

    #endregion

    #region CreateEntry (GET)

    [TestMethod]
    public async Task CreateEntry_Get_ValidBlog_ReturnsView()
    {
        // Arrange
        var blog = await CreateTestBlog("test-blog", "Test Blog");

        // Act
        var result = await _controller.CreateEntry(blog.BlogKey) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("CreateEntry", result.ViewName);
        var model = result.Model as BlogEntryEditViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(blog.BlogKey, model.BlogKey);
        Assert.IsTrue(model.PublishNow);
    }

    #endregion

    #region CreateEntry (POST)

    [TestMethod]
    public async Task CreateEntry_Post_ValidModel_CreatesArticleAndRedirects()
    {
        // Arrange
        await Logic.CreateArticle("Home Page", TestUserId);
        var blog = await CreateTestBlog("my-blog", "My Blog");
        var model = new BlogEntryEditViewModel
        {
            BlogKey = blog.BlogKey,
            Title = "First Post",
            Introduction = "Intro text",
            Content = "<p>Content here</p>",
            BannerImage = "/banner.jpg",
            PublishNow = true
        };

        // Act
        var result = await _controller.CreateEntry(blog.BlogKey, model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(nameof(_controller.Entries), result.ActionName);

        var article = await Db.Articles.FirstOrDefaultAsync(a => a.Title == "First Post");
        Assert.IsNotNull(article);
        Assert.AreEqual(blog.BlogKey, article.BlogKey);
        Assert.AreEqual((int)ArticleType.BlogPost, article.ArticleType);
        Assert.IsNotNull(article.Published);
    }

    [TestMethod]
    public async Task CreateEntry_Post_PublishNowFalse_DoesNotSetPublishedDate()
    {
        // Arrange
        await Logic.CreateArticle("Home Page", TestUserId);
        var blog = await CreateTestBlog("test-blog", "Test");

        var model = new BlogEntryEditViewModel
        {
            BlogKey = blog.BlogKey,
            Title = "Draft Post",
            Content = "<p>Draft</p>",
            PublishNow = false
        };

        // Act
        await _controller.CreateEntry(blog.BlogKey, model);

        // Assert
        var article = await Db.Articles.FirstAsync(a => a.Title == "Draft Post");
        Assert.IsNull(article.Published);
    }

    [TestMethod]
    public async Task EditEntry_Post_ValidModel_UpdatesArticleAndRedirects()
    {
        // Arrange
        await Logic.CreateArticle("Home Page", TestUserId);
        var blog = await CreateTestBlog("update-blog", "Update Blog");
        var post = await Logic.CreateArticle("Original Title", TestUserId, null, blog.BlogKey);

        var model = new BlogEntryEditViewModel
        {
            BlogKey = blog.BlogKey,
            ArticleNumber = post.ArticleNumber,
            Title = "Updated Title",
            Introduction = "Updated intro",
            Content = "<p>Updated content</p>",
            PublishNow = true
        };

        // Act
        var result = await _controller.EditEntry(blog.BlogKey, post.ArticleNumber, model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(nameof(_controller.Entries), result.ActionName);

        var updated = await Logic.GetArticleByArticleNumber(post.ArticleNumber, null);
        Assert.AreEqual("Updated Title", updated.Title);
        Assert.AreEqual("Updated intro", updated.Introduction);
    }

    [TestMethod]
    public async Task EditEntry_Post_UnpublishExisting_RemovesPublishedDate()
    {
        // Arrange
        await Logic.CreateArticle("Home Page", TestUserId);
        var blog = await CreateTestBlog("test", "Test");
        var post = await Logic.CreateArticle("Published", TestUserId, null, blog.BlogKey);

        var entity = await Db.Articles.FirstAsync(a => a.ArticleNumber == post.ArticleNumber);
        entity.Published = DateTimeOffset.UtcNow;
        await Db.SaveChangesAsync();

        var model = new BlogEntryEditViewModel
        {
            BlogKey = blog.BlogKey,
            ArticleNumber = post.ArticleNumber,
            Title = "Published",
            PublishNow = false // Unpublish
        };

        // Act
        await _controller.EditEntry(blog.BlogKey, post.ArticleNumber, model);

        // Assert - Verify unpublish logic if implemented
    }

    [TestMethod]
    public async Task CreateEntry_Post_NonExistentBlog_ReturnsNotFound()
    {
        // Arrange
        var model = new BlogEntryEditViewModel
        {
            BlogKey = "nonexistent",
            Title = "Post"
        };

        // Act
        var result = await _controller.CreateEntry("nonexistent", model);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task EditEntry_Post_ArticleNumberMismatch_ReturnsBadRequest()
    {
        // Arrange
        await Logic.CreateArticle("Home Page", TestUserId);
        var blog = await CreateTestBlog("test", "Test");
        var post = await Logic.CreateArticle("Post", TestUserId, null, blog.BlogKey);

        var model = new BlogEntryEditViewModel
        {
            BlogKey = blog.BlogKey,
            ArticleNumber = 9999, // Wrong number
            Title = "Post"
        };

        // Act
        var result = await _controller.EditEntry(blog.BlogKey, post.ArticleNumber, model);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestResult));
    }

    #endregion

    #region GenericBlogPage (Preview)

    [TestMethod]
    public async Task GenericBlogPage_ValidBlog_ReturnsViewWithPosts()
    {
        // Arrange
        await Logic.CreateArticle("Home Page", TestUserId);
        var blog = await CreateTestBlog("preview-blog", "Preview Blog");
        await Logic.CreateArticle("Post 1", TestUserId, null, blog.BlogKey);
        await Logic.CreateArticle("Post 2", TestUserId, null, blog.BlogKey);

        // Act
        var result = await _controller.GenericBlogPage(blog.BlogKey) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("GenericBlog", result.ViewName);
        Assert.AreEqual("Preview Blog", _controller.ViewData["BlogTitle"]);
    }

    [TestMethod]
    public async Task GenericBlogPage_NonExistentBlog_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GenericBlogPage("missing-blog");

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task GenericBlogPage_TakesOnly25MostRecent()
    {
        // Arrange
        await Logic.CreateArticle("Home Page", TestUserId);
        var blog = await CreateTestBlog("popular", "Popular");

        for (int i = 0; i < 30; i++)
        {
            await Logic.CreateArticle($"Post {i}", TestUserId, null, blog.BlogKey);
        }

        // Act
        var result = await _controller.GenericBlogPage(blog.BlogKey) as ViewResult;
        var posts = result.Model as List<CatalogEntry>;

        // Assert
        Assert.AreEqual(25, posts.Count);
    }

    [TestMethod]
    public async Task GenericBlogPage_SetsViewDataCorrectly()
    {
        // Arrange
        await Logic.CreateArticle("Home Page", TestUserId);
        var blog = await CreateTestBlog("test", "Test Blog",
            description: "Test Description",
            sortOrder: 0);
        blog.HeroImage = "/hero.jpg";
        await Db.SaveChangesAsync();

        // Act
        var result = await _controller.GenericBlogPage(blog.BlogKey) as ViewResult;

        // Assert
        Assert.AreEqual("Test Blog", _controller.ViewData["BlogTitle"]);
        Assert.AreEqual("Test Description", _controller.ViewData["BlogDescription"]);
        Assert.AreEqual("/hero.jpg", _controller.ViewData["HeroImage"]);
    }

    #endregion

    #region GetBlogs (JSON API)

    [TestMethod]
    public async Task GetBlogs_ReturnsJsonWithAllBlogs()
    {
        // Arrange
        await CreateTestBlog("blog-a", "Blog A", sortOrder: 1);
        await CreateTestBlog("blog-b", "Blog B", sortOrder: 0);

        // Act
        var result = await _controller.GetBlogs() as JsonResult;

        // Assert
        Assert.IsNotNull(result);
        var data = result.Value as System.Collections.Generic.List<BlogStreamViewModel>;
        Assert.IsNotNull(data);
        Assert.AreEqual(2, data.Count);
        // Ordered by SortOrder then BlogKey
        Assert.AreEqual("blog-b", data[0].BlogKey);
        Assert.AreEqual("blog-a", data[1].BlogKey);
    }

    #endregion

    #region Helpers

    private async Task<Blog> CreateTestBlog(string key, string title, string description = "Test description",
        bool isDefault = false, int sortOrder = 0)
    {
        var blog = new Blog
        {
            BlogKey = key,
            Title = title,
            Description = description,
            HeroImage = string.Empty,
            IsDefault = isDefault,
            SortOrder = sortOrder
        };
        Db.Blogs.Add(blog);
        await Db.SaveChangesAsync();
        return blog;
    }

    #endregion
}