using Sky.Cms.Models;
using Sky.Editor.Services.ReservedPaths;

namespace Sky.Tests.Services.Paths;

/// <summary>
/// Unit tests for ReservedPaths service covering CRUD operations and validation.
/// </summary>
[DoNotParallelize]
[TestClass]
public class ReservedPathsTests : SkyCmsTestBase
{
    private ReservedPaths _reservedPaths;

    [TestInitialize]
    public void Setup()
    {
        InitializeTestContext(seedLayout: true);
        _reservedPaths = new ReservedPaths(Db);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Db.Dispose();
    }

    #region GetReservedPaths

    [TestMethod]
    public async Task GetReservedPaths_SubsequentCall_ReturnsPersistedPaths()
    {
        // Arrange - First call initializes
        await _reservedPaths.GetReservedPaths();

        // Act - Second call should read from DB
        var paths = await _reservedPaths.GetReservedPaths();

        // Assert
        Assert.IsNotNull(paths);
        Assert.IsNotEmpty(paths);
    }

    #endregion

    #region IsReserved

    [TestMethod]
    public async Task IsReserved_SystemPath_ReturnsTrue()
    {
        // Act
        var isReserved = await _reservedPaths.IsReserved("root");

        // Assert
        Assert.IsTrue(isReserved);
    }

    [TestMethod]
    public async Task IsReserved_CaseInsensitive_ReturnsTrue()
    {
        // Act
        var isReservedLower = await _reservedPaths.IsReserved("admin");
        var isReservedUpper = await _reservedPaths.IsReserved("ADMIN");
        var isReservedMixed = await _reservedPaths.IsReserved("AdMiN");

        // Assert
        Assert.IsTrue(isReservedLower);
        Assert.IsTrue(isReservedUpper);
        Assert.IsTrue(isReservedMixed);
    }

    [TestMethod]
    public async Task IsReserved_NonReservedPath_ReturnsFalse()
    {
        // Act
        var isReserved = await _reservedPaths.IsReserved("my-custom-page");

        // Assert
        Assert.IsFalse(isReserved);
    }

    [TestMethod]
    public async Task IsReserved_EmptyString_ReturnsFalse()
    {
        // Act
        var isReserved = await _reservedPaths.IsReserved(string.Empty);

        // Assert
        Assert.IsFalse(isReserved);
    }

    [TestMethod]
    public async Task IsReserved_NestedPath_ReturnsTrue()
    {
        // Act
        var isReserved = await _reservedPaths.IsReserved("blog/rss");

        // Assert
        Assert.IsTrue(isReserved);
    }

    #endregion

    #region Upsert - Add New Path

    [TestMethod]
    public async Task Upsert_NewPath_AddsToList()
    {
        // Arrange
        var newPath = new ReservedPath
        {
            Id = Guid.NewGuid(),
            Path = "custom-reserved",
            CosmosRequired = false,
            Notes = "Custom reserved path for testing"
        };

        // Act
        await _reservedPaths.Upsert(newPath);
        await Db.SaveChangesAsync();

        // Assert
        var paths = await _reservedPaths.GetReservedPaths();
        var added = paths.FirstOrDefault(p => p.Path == "custom-reserved");
        Assert.IsNotNull(added);
        Assert.AreEqual("Custom reserved path for testing", added.Notes);
        Assert.IsFalse(added.CosmosRequired);
    }

    [TestMethod]
    public async Task Upsert_NewPath_IsReservedReturnsTrue()
    {
        // Arrange
        var newPath = new ReservedPath
        {
            Id = Guid.NewGuid(),
            Path = "new-reserved-path",
            CosmosRequired = false,
            Notes = "Test path"
        };

        // Act
        await _reservedPaths.Upsert(newPath);
        await Db.SaveChangesAsync();

        // Assert
        var isReserved = await _reservedPaths.IsReserved("new-reserved-path");
        Assert.IsTrue(isReserved);
    }

    #endregion

    #region Upsert - Update Existing Path

    [TestMethod]
    public async Task Upsert_ExistingNonSystemPath_UpdatesSuccessfully()
    {
        // Arrange - Add a custom path
        var customPath = new ReservedPath
        {
            Id = Guid.NewGuid(),
            Path = "updatable-path",
            CosmosRequired = false,
            Notes = "Original notes"
        };
        await _reservedPaths.Upsert(customPath);
        await Db.SaveChangesAsync();

        // Act - Update the same path
        var updatedPath = new ReservedPath
        {
            Id = Guid.NewGuid(),
            Path = "updatable-path",
            CosmosRequired = false,
            Notes = "Updated notes"
        };
        await _reservedPaths.Upsert(updatedPath);
        await Db.SaveChangesAsync();

        // Assert
        var paths = await _reservedPaths.GetReservedPaths();
        var found = paths.FirstOrDefault(p => p.Path == "updatable-path");
        Assert.IsNotNull(found);
        Assert.AreEqual("Updated notes", found.Notes);
        Assert.IsFalse(found.CosmosRequired); // Should remain false (line 50 sets it to false)
    }

    [TestMethod]
    public async Task Upsert_ExistingSystemPath_ThrowsException()
    {
        // Arrange
        var systemPath = new ReservedPath
        {
            Path = "root", // System required path
            CosmosRequired = true,
            Notes = "Attempt to modify system path"
        };

        // Act & Assert
        await Assert.ThrowsExactlyAsync<Exception>(
            async () => await _reservedPaths.Upsert(systemPath),
            "Cannot update a system required path.");
    }

    [TestMethod]
    public async Task Upsert_CaseInsensitiveMatch_UpdatesCorrectPath()
    {
        // Arrange
        var customPath = new ReservedPath
        {
            Id = Guid.NewGuid(),
            Path = "my-path",
            CosmosRequired = false,
            Notes = "Original"
        };
        await _reservedPaths.Upsert(customPath);
        await Db.SaveChangesAsync();

        // Act - Update with different case
        var updated = new ReservedPath
        {
            Id = Guid.NewGuid(),
            Path = "MY-PATH",
            CosmosRequired = false,
            Notes = "Updated via uppercase"
        };
        await _reservedPaths.Upsert(updated);
        await Db.SaveChangesAsync();

        // Assert
        var paths = await _reservedPaths.GetReservedPaths();
        var matches = paths.Where(p => p.Path.ToLower() == "my-path").ToList();
        Assert.HasCount(1, matches, "Should only have one entry");
        Assert.AreEqual("Updated via uppercase", matches[0].Notes);
    }

    #endregion

    #region Remove

    [TestMethod]
    public async Task Remove_CustomPath_RemovesSuccessfully()
    {
        // Arrange
        var customPath = new ReservedPath
        {
            Id = Guid.NewGuid(),
            Path = "removable-path",
            CosmosRequired = false,
            Notes = "Will be removed"
        };
        await _reservedPaths.Upsert(customPath);
        await Db.SaveChangesAsync();

        // Act
        await _reservedPaths.Remove("removable-path");

        // Assert
        var paths = await _reservedPaths.GetReservedPaths();
        var found = paths.FirstOrDefault(p => p.Path == "removable-path");
        Assert.IsNull(found);

        var isReserved = await _reservedPaths.IsReserved("removable-path");
        Assert.IsFalse(isReserved);
    }

    [TestMethod]
    public async Task Remove_SystemRequiredPath_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsExactlyAsync<Exception>(
            async () => await _reservedPaths.Remove("root"),
            "Cannot remove a system required path.");
    }

    [TestMethod]
    public async Task Remove_CaseInsensitive_RemovesCorrectPath()
    {
        // Arrange
        var customPath = new ReservedPath
        {
            Id = Guid.NewGuid(),
            Path = "case-test",
            CosmosRequired = false,
            Notes = "Test"
        };
        await _reservedPaths.Upsert(customPath);
        await Db.SaveChangesAsync();

        // Act - Remove with different case
        await _reservedPaths.Remove("CASE-TEST");

        // Assert
        var paths = await _reservedPaths.GetReservedPaths();
        Assert.IsFalse(paths.Any(p => p.Path.ToLower() == "case-test"));
    }

    [TestMethod]
    public async Task Remove_NonExistentPath_NoException()
    {
        // Act - Should not throw
        await _reservedPaths.Remove("non-existent-path");

        // Assert - If we get here, no exception was thrown

    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public async Task Upsert_PathWithTrailingSlash_HandledCorrectly()
    {
        // Arrange
        var pathWithSlash = new ReservedPath
        {
            Id = Guid.NewGuid(),
            Path = "test-path/",
            CosmosRequired = false,
            Notes = "Has trailing slash"
        };

        // Act
        await _reservedPaths.Upsert(pathWithSlash);
        await Db.SaveChangesAsync();

        // Assert
        var isReserved = await _reservedPaths.IsReserved("test-path/");
        Assert.IsTrue(isReserved);
    }

    [TestMethod]
    public async Task GetReservedPaths_AfterMultipleOperations_MaintainsConsistency()
    {
        // Arrange & Act
        var initialCount = (await _reservedPaths.GetReservedPaths()).Count;

        // Add
        await _reservedPaths.Upsert(new ReservedPath
        {
            Id = Guid.NewGuid(),
            Path = "temp1",
            CosmosRequired = false,
            Notes = "Temp"
        });
        await Db.SaveChangesAsync();
        var afterAdd = (await _reservedPaths.GetReservedPaths()).Count;

        // Remove
        await _reservedPaths.Remove("temp1");
        var afterRemove = (await _reservedPaths.GetReservedPaths()).Count;

        // Assert
        Assert.AreEqual(initialCount + 1, afterAdd);
        Assert.AreEqual(initialCount, afterRemove);
    }

    #endregion

    #region Notes and Metadata

    [TestMethod]
    public async Task GetReservedPaths_AllPathsHaveNotes()
    {
        // Act
        var paths = await _reservedPaths.GetReservedPaths();

        // Assert
        foreach (var path in paths)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(path.Notes),
                $"Path '{path.Path}' should have notes");
        }
    }

    [TestMethod]
    public async Task Upsert_NewPath_PreservesAllProperties()
    {
        // Arrange
        var testId = Guid.NewGuid();
        var newPath = new ReservedPath
        {
            Id = testId,
            Path = "property-test",
            CosmosRequired = false,
            Notes = "Testing all properties"
        };

        // Act
        await _reservedPaths.Upsert(newPath);
        await Db.SaveChangesAsync();

        // Assert
        var paths = await _reservedPaths.GetReservedPaths();
        var found = paths.FirstOrDefault(p => p.Path == "property-test");
        Assert.IsNotNull(found);
        Assert.AreEqual("property-test", found.Path);
        Assert.IsFalse(found.CosmosRequired);
        Assert.AreEqual("Testing all properties", found.Notes);
    }

    #endregion
}