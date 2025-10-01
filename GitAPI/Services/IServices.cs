using Sky.GitAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sky.GitAPI.Services
{
    /// <summary>
    /// Service interface for Git operations
    /// </summary>
    public interface IGitService
    {
        /// <summary>
        /// Generate SHA-1 hash for content
        /// </summary>
        string GenerateSha(string content);

        /// <summary>
        /// Generate SHA-1 hash for Git object
        /// </summary>
        string GenerateGitObjectSha(string type, string content);

        /// <summary>
        /// Create a Git URL for the given resource
        /// </summary>
        string CreateUrl(string baseUrl, string path);
    }

    /// <summary>
    /// Service interface for Article Git operations
    /// </summary>
    public interface IArticleGitService
    {
        /// <summary>
        /// Get all articles as Git tree entries
        /// </summary>
        Task<List<GitTreeEntry>> GetArticleTreeEntriesAsync();

        /// <summary>
        /// Get article content as Git blob
        /// </summary>
        Task<GitObject?> GetArticleBlobAsync(int articleNumber, int? version = null);

        /// <summary>
        /// Get article metadata
        /// </summary>
        Task<ArticleGitMetadata?> GetArticleMetadataAsync(int articleNumber, int? version = null);

        /// <summary>
        /// Update article content from Git blob
        /// </summary>
        Task<bool> UpdateArticleFromBlobAsync(int articleNumber, string content, string author, string message);

        /// <summary>
        /// Create new article from Git blob
        /// </summary>
        Task<ArticleGitMetadata> CreateArticleFromBlobAsync(string title, string content, string author);

        /// <summary>
        /// Get article versions
        /// </summary>
        Task<List<ArticleGitMetadata>> GetArticleVersionsAsync(int articleNumber);

        /// <summary>
        /// Convert article to file path
        /// </summary>
        string GetArticleFilePath(string title, int articleNumber);

        /// <summary>
        /// Parse file path to get article info
        /// </summary>
        (int articleNumber, string title) ParseArticleFilePath(string filePath);
    }
}