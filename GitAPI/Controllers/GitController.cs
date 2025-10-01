using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sky.GitAPI.Models;
using Sky.GitAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sky.GitAPI.Controllers
{
    /// <summary>
    /// Git API controller for basic Git operations
    /// </summary>
    [ApiController]
    [Route("api/git")]
    [Authorize]
    public class GitController : ControllerBase
    {
        private readonly IArticleGitService _articleGitService;
        private readonly IGitService _gitService;
        private readonly GitApiSettings _settings;
        private readonly ILogger<GitController> _logger;

        public GitController(
            IArticleGitService articleGitService,
            IGitService gitService,
            IOptions<GitApiSettings> settings,
            ILogger<GitController> logger)
        {
            _articleGitService = articleGitService;
            _gitService = gitService;
            _settings = settings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Get repository references (branches/tags)
        /// </summary>
        [HttpGet("refs")]
        public async Task<IActionResult> GetRefs()
        {
            try
            {
                // For simplicity, we'll return a single main branch
                var refs = new[]
                {
                    new GitRef
                    {
                        Ref = $"refs/heads/{_settings.DefaultBranch}",
                        Sha = await GetLatestCommitShaAsync(),
                        Url = _gitService.CreateUrl(_settings.BaseUrl, $"api/git/refs/heads/{_settings.DefaultBranch}")
                    }
                };

                return Ok(refs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repository references");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get a specific reference
        /// </summary>
        [HttpGet("refs/{*refPath}")]
        public async Task<IActionResult> GetRef(string refPath)
        {
            try
            {
                if (refPath == $"heads/{_settings.DefaultBranch}")
                {
                    var gitRef = new GitRef
                    {
                        Ref = $"refs/heads/{_settings.DefaultBranch}",
                        Sha = await GetLatestCommitShaAsync(),
                        Url = _gitService.CreateUrl(_settings.BaseUrl, $"api/git/refs/heads/{_settings.DefaultBranch}")
                    };

                    return Ok(gitRef);
                }

                return NotFound($"Reference refs/{refPath} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reference {RefPath}", refPath);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get repository tree (list of files)
        /// </summary>
        [HttpGet("trees/{sha}")]
        public async Task<IActionResult> GetTree(string sha, [FromQuery] bool recursive = false)
        {
            try
            {
                var entries = await _articleGitService.GetArticleTreeEntriesAsync();

                var tree = new GitTree
                {
                    Sha = sha,
                    Url = _gitService.CreateUrl(_settings.BaseUrl, $"api/git/trees/{sha}"),
                    Tree = entries,
                    Truncated = false
                };

                return Ok(tree);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tree {Sha}", sha);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get a blob (file content)
        /// </summary>
        [HttpGet("blobs/{sha}")]
        public async Task<IActionResult> GetBlob(string sha)
        {
            try
            {
                // Try to find article by SHA
                var entries = await _articleGitService.GetArticleTreeEntriesAsync();
                var entry = entries.FirstOrDefault(e => e.Sha == sha);

                if (entry == null)
                {
                    return NotFound($"Blob {sha} not found");
                }

                // Parse article info from path
                var (articleNumber, _) = _articleGitService.ParseArticleFilePath(entry.Path);
                var blob = await _articleGitService.GetArticleBlobAsync(articleNumber);

                if (blob == null)
                {
                    return NotFound($"Blob {sha} not found");
                }

                return Ok(blob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blob {Sha}", sha);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new blob
        /// </summary>
        [HttpPost("blobs")]
        public async Task<IActionResult> CreateBlob([FromBody] CreateBlobRequest request)
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

                var sha = _gitService.GenerateGitObjectSha("blob", content);

                var blob = new GitObject
                {
                    Sha = sha,
                    Type = "blob",
                    Url = _gitService.CreateUrl(_settings.BaseUrl, $"api/git/blobs/{sha}"),
                    Size = System.Text.Encoding.UTF8.GetByteCount(content)
                };

                return Created(blob.Url, blob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blob");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get commit information
        /// </summary>
        [HttpGet("commits/{sha}")]
        public async Task<IActionResult> GetCommit(string sha)
        {
            try
            {
                var treeSha = await GetLatestTreeShaAsync();
                
                var commit = new GitCommit
                {
                    Sha = sha,
                    Url = _gitService.CreateUrl(_settings.BaseUrl, $"api/git/commits/{sha}"),
                    Commit = new GitCommitDetails
                    {
                        Message = "Latest articles state",
                        Author = new GitAuthor
                        {
                            Name = "Sky CMS",
                            Email = "system@skycms.com",
                            Date = DateTimeOffset.UtcNow
                        },
                        Committer = new GitAuthor
                        {
                            Name = "Sky CMS",
                            Email = "system@skycms.com",
                            Date = DateTimeOffset.UtcNow
                        },
                        Tree = new GitTree
                        {
                            Sha = treeSha,
                            Url = _gitService.CreateUrl(_settings.BaseUrl, $"api/git/trees/{treeSha}")
                        }
                    },
                    Parents = new List<GitCommit>()
                };

                return Ok(commit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting commit {Sha}", sha);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update a reference (used for pushing changes)
        /// </summary>
        [HttpPatch("refs/{*refPath}")]
        public async Task<IActionResult> UpdateRef(string refPath, [FromBody] UpdateRefRequest request)
        {
            try
            {
                if (refPath != $"heads/{_settings.DefaultBranch}")
                {
                    return BadRequest($"Cannot update reference refs/{refPath}");
                }

                // In a real implementation, you would validate the SHA and update accordingly
                // For now, we'll just return the updated reference
                var gitRef = new GitRef
                {
                    Ref = $"refs/heads/{_settings.DefaultBranch}",
                    Sha = request.Sha,
                    Url = _gitService.CreateUrl(_settings.BaseUrl, $"api/git/refs/heads/{_settings.DefaultBranch}")
                };

                return Ok(gitRef);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reference {RefPath}", refPath);
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<string> GetLatestCommitShaAsync()
        {
            // Generate a SHA based on the current state of articles
            var entries = await _articleGitService.GetArticleTreeEntriesAsync();
            var content = string.Join("\n", entries.Select(e => $"{e.Sha} {e.Path}"));
            return _gitService.GenerateSha($"commit {content}");
        }

        private async Task<string> GetLatestTreeShaAsync()
        {
            // Generate a SHA based on the current tree state
            var entries = await _articleGitService.GetArticleTreeEntriesAsync();
            var content = string.Join("\n", entries.Select(e => $"{e.Mode} {e.Type} {e.Sha}\t{e.Path}"));
            return _gitService.GenerateSha($"tree {content}");
        }
    }
}