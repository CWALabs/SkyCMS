// <copyright file="ArticleEditLogicTests.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Sky.Editor.Features.Articles.Save;

namespace Sky.Tests.Logic;

[DoNotParallelize]
[TestClass]
public class ArticleEditLogicTests : SkyCmsTestBase
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
        var valid = await TitleChangeService.ValidateTitle("Duplicate", null);
        Assert.IsFalse(valid, "Expected duplicate title to be invalid");
    }

    [TestMethod]
    public async Task SaveArticle_UpdatesTitleAndContent()
    {
        // Arrange
        var vm = await Logic.CreateArticle("Original", TestUserId);

        // MIGRATED: Use SaveArticleHandler
        var command = new SaveArticleCommand
        {
            ArticleNumber = vm.ArticleNumber,
            Title = "Original Updated",
            Content = "<div contenteditable='true' data-ccms-ceid='x'>Updated</div>",
            UserId = TestUserId,
            ArticleType = vm.ArticleType
        };

        // Act
        var result = await SaveArticleHandler.HandleAsync(command);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Data.ServerSideSuccess);

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
