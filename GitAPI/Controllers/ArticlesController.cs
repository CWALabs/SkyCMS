using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sky.GitAPI.Models;
using Sky.GitAPI.Services;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sky.GitAPI.Controllers
{
    /// <summary>
    /// Article-specific Git API controller
    /// </summary>
    [ApiController]
    [Route("api/articles")]
    [Authorize]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleGitService _articleGitService;
        private readonly ILogger<ArticlesController> _logger;

        public ArticlesController(
            IArticleGitService articleGitService,
            ILogger<ArticlesController> logger)
        {
            _articleGitService = articleGitService;
            _logger = logger;
        }

        /// <summary>
        /// Get all articles
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetArticles()
        {
            try
            {
                var entries = await _articleGitService.GetArticleTreeEntriesAsync();
                return Ok(entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting articles");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get a specific article
        /// </summary>
        [HttpGet("{articleNumber}")]
        public async Task<IActionResult> GetArticle(int articleNumber, [FromQuery] int? version = null)
        {
            try
            {
                var metadata = await _articleGitService.GetArticleMetadataAsync(articleNumber, version);
                if (metadata == null)
                {
                    return NotFound($"Article {articleNumber} not found");
                }

                var blob = await _articleGitService.GetArticleBlobAsync(articleNumber, version);
                if (blob == null)
                {
                    return NotFound($"Article {articleNumber} content not found");
                }

                return Ok(new
                {
                    Metadata = metadata,
                    Content = blob
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting article {ArticleNumber}", articleNumber);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get article versions
        /// </summary>
        [HttpGet("{articleNumber}/versions")]
        public async Task<IActionResult> GetArticleVersions(int articleNumber)
        {
            try
            {
                var versions = await _articleGitService.GetArticleVersionsAsync(articleNumber);
                return Ok(versions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting article versions for {ArticleNumber}", articleNumber);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update an article
        /// </summary>
        [HttpPut("{articleNumber}")]
        public async Task<IActionResult> UpdateArticle(int articleNumber, [FromBody] CreateBlobRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Content))
                {
                    return BadRequest("Content is required");
                }

                // Decode content if it's base64
                string content = request.Content;
                if (request.Encoding == "base64")
                {
                    var bytes = Convert.FromBase64String(request.Content);
                    content = System.Text.Encoding.UTF8.GetString(bytes);
                }

                var author = User.Identity?.Name ?? "system";
                var success = await _articleGitService.UpdateArticleFromBlobAsync(
                    articleNumber, 
                    content, 
                    author, 
                    $"Updated article {articleNumber} via Git API");

                if (!success)
                {
                    return NotFound($"Article {articleNumber} not found");
                }

                var metadata = await _articleGitService.GetArticleMetadataAsync(articleNumber);
                return Ok(metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating article {ArticleNumber}", articleNumber);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new article
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateArticle([FromBody] CreateArticleRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Content))
                {
                    return BadRequest("Title and Content are required");
                }

                // Decode content if it's base64
                string content = request.Content;
                if (request.Encoding == "base64")
                {
                    var bytes = Convert.FromBase64String(request.Content);
                    content = System.Text.Encoding.UTF8.GetString(bytes);
                }

                var author = User.Identity?.Name ?? "system";
                var metadata = await _articleGitService.CreateArticleFromBlobAsync(
                    request.Title, 
                    content, 
                    author);

                return Created($"/api/articles/{metadata.ArticleNumber}", metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating article");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    /// <summary>
    /// Request model for creating articles
    /// </summary>
    public class CreateArticleRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Encoding { get; set; } = "utf-8";
    }
}