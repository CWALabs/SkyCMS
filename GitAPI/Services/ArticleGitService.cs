using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Cosmos.Common.Data;
using Cosmos.Common.Models;
using Cosmos.Common.Data.Logic;
using Sky.GitAPI.Models;
using System.Text.RegularExpressions;
using System.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sky.GitAPI.Services
{
    /// <summary>
    /// Service for Article Git operations
    /// </summary>
    public class ArticleGitService : IArticleGitService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IGitService _gitService;
        private readonly GitApiSettings _settings;

        public ArticleGitService(
            ApplicationDbContext dbContext,
            IGitService gitService,
            IOptions<GitApiSettings> settings)
        {
            _dbContext = dbContext;
            _gitService = gitService;
            _settings = settings.Value;
        }

        public async Task<List<GitTreeEntry>> GetArticleTreeEntriesAsync()
        {
            var articles = await _dbContext.Articles
                .Where(a => a.StatusCode == (int)StatusCodeEnum.Active)
                .GroupBy(a => a.ArticleNumber)
                .Select(g => g.OrderByDescending(a => a.VersionNumber).First())
                .ToListAsync();

            var entries = new List<GitTreeEntry>();

            foreach (var article in articles)
            {
                var filePath = GetArticleFilePath(article.Title, article.ArticleNumber);
                var content = GetArticleFileContent(article);
                var sha = _gitService.GenerateGitObjectSha("blob", content);

                entries.Add(new GitTreeEntry
                {
                    Path = filePath,
                    Mode = "100644",
                    Type = "blob",
                    Sha = sha,
                    Url = _gitService.CreateUrl(_settings.BaseUrl, $"api/git/blobs/{sha}"),
                    Size = System.Text.Encoding.UTF8.GetByteCount(content)
                });
            }

            return entries;
        }

        public async Task<GitObject?> GetArticleBlobAsync(int articleNumber, int? version = null)
        {
            var query = _dbContext.Articles.Where(a => a.ArticleNumber == articleNumber);
            
            if (version.HasValue)
            {
                query = query.Where(a => a.VersionNumber == version.Value);
            }
            else
            {
                query = query.OrderByDescending(a => a.VersionNumber);
            }

            var article = await query.FirstOrDefaultAsync();
            if (article == null) return null;

            var content = GetArticleFileContent(article);
            var sha = _gitService.GenerateGitObjectSha("blob", content);

            return new GitObject
            {
                Sha = sha,
                Type = "blob",
                Url = _gitService.CreateUrl(_settings.BaseUrl, $"api/git/blobs/{sha}"),
                Size = System.Text.Encoding.UTF8.GetByteCount(content),
                Content = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content)),
                Encoding = "base64"
            };
        }

        public async Task<ArticleGitMetadata?> GetArticleMetadataAsync(int articleNumber, int? version = null)
        {
            var query = _dbContext.Articles.Where(a => a.ArticleNumber == articleNumber);
            
            if (version.HasValue)
            {
                query = query.Where(a => a.VersionNumber == version.Value);
            }
            else
            {
                query = query.OrderByDescending(a => a.VersionNumber);
            }

            var article = await query.FirstOrDefaultAsync();
            if (article == null) return null;

            var content = GetArticleFileContent(article);
            var filePath = GetArticleFilePath(article.Title, article.ArticleNumber);

            return new ArticleGitMetadata
            {
                ArticleNumber = article.ArticleNumber,
                Title = article.Title,
                UrlPath = article.UrlPath,
                VersionNumber = article.VersionNumber,
                Id = article.Id,
                LastModified = article.Updated,
                Author = article.UserId ?? "system",
                FilePath = filePath,
                Sha = _gitService.GenerateGitObjectSha("blob", content)
            };
        }

        public async Task<bool> UpdateArticleFromBlobAsync(int articleNumber, string content, string author, string message)
        {
            var article = await _dbContext.Articles
                .Where(a => a.ArticleNumber == articleNumber)
                .OrderByDescending(a => a.VersionNumber)
                .FirstOrDefaultAsync();

            if (article == null) return false;

            // Parse the content to extract article parts
            var parsed = ParseArticleFileContent(content);

            // Create new version if current version is published
            if (article.Published.HasValue)
            {
                var newVersion = new Article
                {
                    Id = Guid.NewGuid(),
                    ArticleNumber = article.ArticleNumber,
                    VersionNumber = article.VersionNumber + 1,
                    Title = parsed.Title ?? article.Title,
                    Content = parsed.Content,
                    HeaderJavaScript = parsed.HeadJavaScript,
                    FooterJavaScript = parsed.FooterJavaScript,
                    UrlPath = article.UrlPath,
                    StatusCode = article.StatusCode,
                    Updated = DateTimeOffset.UtcNow,
                    UserId = author,
                    Published = null,
                    BannerImage = article.BannerImage,
                    Expires = article.Expires
                };

                _dbContext.Articles.Add(newVersion);
            }
            else
            {
                // Update current unpublished version
                article.Title = parsed.Title ?? article.Title;
                article.Content = parsed.Content;
                article.HeaderJavaScript = parsed.HeadJavaScript;
                article.FooterJavaScript = parsed.FooterJavaScript;
                article.Updated = DateTimeOffset.UtcNow;
                article.UserId = author;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<ArticleGitMetadata> CreateArticleFromBlobAsync(string title, string content, string author)
        {
            var parsed = ParseArticleFileContent(content);
            var nextArticleNumber = await GetNextArticleNumberAsync();

            var article = new Article
            {
                Id = Guid.NewGuid(),
                ArticleNumber = nextArticleNumber,
                VersionNumber = 1,
                Title = parsed.Title ?? title,
                Content = parsed.Content,
                HeaderJavaScript = parsed.HeadJavaScript,
                FooterJavaScript = parsed.FooterJavaScript,
                UrlPath = GenerateUrlPath(parsed.Title ?? title),
                StatusCode = (int)StatusCodeEnum.Active,
                Updated = DateTimeOffset.UtcNow,
                UserId = author,
                Published = null
            };

            _dbContext.Articles.Add(article);
            await _dbContext.SaveChangesAsync();

            var filePath = GetArticleFilePath(article.Title, article.ArticleNumber);
            var fullContent = GetArticleFileContent(article);

            return new ArticleGitMetadata
            {
                ArticleNumber = article.ArticleNumber,
                Title = article.Title,
                UrlPath = article.UrlPath,
                VersionNumber = article.VersionNumber,
                Id = article.Id,
                LastModified = article.Updated,
                Author = author,
                FilePath = filePath,
                Sha = _gitService.GenerateGitObjectSha("blob", fullContent)
            };
        }

        public async Task<List<ArticleGitMetadata>> GetArticleVersionsAsync(int articleNumber)
        {
            var articles = await _dbContext.Articles
                .Where(a => a.ArticleNumber == articleNumber)
                .OrderByDescending(a => a.VersionNumber)
                .ToListAsync();

            return articles.Select(article =>
            {
                var content = GetArticleFileContent(article);
                var filePath = GetArticleFilePath(article.Title, article.ArticleNumber);

                return new ArticleGitMetadata
                {
                    ArticleNumber = article.ArticleNumber,
                    Title = article.Title,
                    UrlPath = article.UrlPath,
                    VersionNumber = article.VersionNumber,
                    Id = article.Id,
                    LastModified = article.Updated,
                    Author = article.UserId ?? "system",
                    FilePath = filePath,
                    Sha = _gitService.GenerateGitObjectSha("blob", content)
                };
            }).ToList();
        }

        public string GetArticleFilePath(string title, int articleNumber)
        {
            // Sanitize title for filename
            var sanitized = Regex.Replace(title, @"[^\w\s-]", "")
                                 .Trim()
                                 .Replace(" ", "-")
                                 .Replace("--", "-")
                                 .ToLowerInvariant();

            if (string.IsNullOrEmpty(sanitized))
            {
                sanitized = $"article-{articleNumber}";
            }

            return $"articles/{sanitized}-{articleNumber}.html";
        }

        public (int articleNumber, string title) ParseArticleFilePath(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var match = Regex.Match(fileName, @"^(.+)-(\d+)$");
            
            if (match.Success)
            {
                var title = match.Groups[1].Value.Replace("-", " ");
                var articleNumber = int.Parse(match.Groups[2].Value);
                return (articleNumber, title);
            }

            // Fallback parsing
            var parts = fileName.Split('-');
            if (parts.Length > 1 && int.TryParse(parts.Last(), out var number))
            {
                var titleParts = parts.Take(parts.Length - 1);
                var reconstructedTitle = string.Join(" ", titleParts);
                return (number, reconstructedTitle);
            }

            throw new ArgumentException($"Invalid article file path format: {filePath}");
        }

        private string GetArticleFileContent(Article article)
        {
            var content = new System.Text.StringBuilder();
            
            // Add front matter (YAML-like metadata)
            content.AppendLine("---");
            content.AppendLine($"title: \"{article.Title}\"");
            content.AppendLine($"articleNumber: {article.ArticleNumber}");
            content.AppendLine($"version: {article.VersionNumber}");
            content.AppendLine($"urlPath: \"{article.UrlPath}\"");
            content.AppendLine($"updated: \"{article.Updated:yyyy-MM-ddTHH:mm:ssZ}\"");
            content.AppendLine($"author: \"{article.UserId ?? "system"}\"");
            content.AppendLine($"published: {(article.Published?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? "null")}");
            content.AppendLine($"status: {article.StatusCode}");
            
            if (!string.IsNullOrEmpty(article.BannerImage))
            {
                content.AppendLine($"bannerImage: \"{article.BannerImage}\"");
            }
            
            if (article.Expires.HasValue)
            {
                content.AppendLine($"expires: \"{article.Expires:yyyy-MM-ddTHH:mm:ssZ}\"");
            }
            
            content.AppendLine("---");
            content.AppendLine();

            // Add head JavaScript if present
            if (!string.IsNullOrEmpty(article.HeaderJavaScript))
            {
                content.AppendLine("<!-- HEAD_JAVASCRIPT_START -->");
                content.AppendLine(article.HeaderJavaScript);
                content.AppendLine("<!-- HEAD_JAVASCRIPT_END -->");
                content.AppendLine();
            }

            // Add main content
            content.AppendLine("<!-- CONTENT_START -->");
            content.AppendLine(article.Content ?? "");
            content.AppendLine("<!-- CONTENT_END -->");

            // Add footer JavaScript if present
            if (!string.IsNullOrEmpty(article.FooterJavaScript))
            {
                content.AppendLine();
                content.AppendLine("<!-- FOOTER_JAVASCRIPT_START -->");
                content.AppendLine(article.FooterJavaScript);
                content.AppendLine("<!-- FOOTER_JAVASCRIPT_END -->");
            }

            return content.ToString();
        }

        private (string? Title, string Content, string? HeadJavaScript, string? FooterJavaScript) ParseArticleFileContent(string content)
        {
            string? title = null;
            string? headJs = null;
            string? footerJs = null;
            string mainContent = content;

            // Parse front matter
            var frontMatterMatch = Regex.Match(content, @"^---\s*\r?\n(.*?)\r?\n---\s*\r?\n", RegexOptions.Singleline);
            if (frontMatterMatch.Success)
            {
                var frontMatter = frontMatterMatch.Groups[1].Value;
                var titleMatch = Regex.Match(frontMatter, @"title:\s*""(.+?)""");
                if (titleMatch.Success)
                {
                    title = titleMatch.Groups[1].Value;
                }

                // Remove front matter from content
                mainContent = content.Substring(frontMatterMatch.Length);
            }

            // Extract head JavaScript
            var headJsMatch = Regex.Match(mainContent, @"<!-- HEAD_JAVASCRIPT_START -->\s*\r?\n(.*?)\r?\n<!-- HEAD_JAVASCRIPT_END -->", RegexOptions.Singleline);
            if (headJsMatch.Success)
            {
                headJs = headJsMatch.Groups[1].Value.Trim();
                mainContent = mainContent.Replace(headJsMatch.Value, "").Trim();
            }

            // Extract footer JavaScript
            var footerJsMatch = Regex.Match(mainContent, @"<!-- FOOTER_JAVASCRIPT_START -->\s*\r?\n(.*?)\r?\n<!-- FOOTER_JAVASCRIPT_END -->", RegexOptions.Singleline);
            if (footerJsMatch.Success)
            {
                footerJs = footerJsMatch.Groups[1].Value.Trim();
                mainContent = mainContent.Replace(footerJsMatch.Value, "").Trim();
            }

            // Extract main content
            var contentMatch = Regex.Match(mainContent, @"<!-- CONTENT_START -->\s*\r?\n(.*?)\r?\n<!-- CONTENT_END -->", RegexOptions.Singleline);
            if (contentMatch.Success)
            {
                mainContent = contentMatch.Groups[1].Value.Trim();
            }

            return (title, mainContent, headJs, footerJs);
        }

        private async Task<int> GetNextArticleNumberAsync()
        {
            var maxNumber = await _dbContext.Articles
                .MaxAsync(a => (int?)a.ArticleNumber) ?? 0;
            return maxNumber + 1;
        }

        private string GenerateUrlPath(string title)
        {
            return Regex.Replace(title.ToLowerInvariant(), @"[^\w\s-]", "")
                        .Trim()
                        .Replace(" ", "-")
                        .Replace("--", "-");
        }
    }
}