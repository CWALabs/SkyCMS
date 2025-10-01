using System;
using System.Collections.Generic;

namespace Sky.GitAPI.Models
{
    /// <summary>
    /// Git API configuration settings
    /// </summary>
    public class GitApiSettings
    {
        /// <summary>
        /// Basic authentication username
        /// </summary>
        public string Username { get; set; } = "admin";

        /// <summary>
        /// Basic authentication password
        /// </summary>
        public string Password { get; set; } = "password";

        /// <summary>
        /// Repository name for Git API
        /// </summary>
        public string RepositoryName { get; set; } = "skycms-articles";

        /// <summary>
        /// Base URL for the Git API
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:5000";

        /// <summary>
        /// Default branch name
        /// </summary>
        public string DefaultBranch { get; set; } = "main";
    }

    /// <summary>
    /// Represents a Git repository reference
    /// </summary>
    public class GitRef
    {
        public string Ref { get; set; } = string.Empty;
        public string Sha { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a Git object (blob, tree, commit)
    /// </summary>
    public class GitObject
    {
        public string Sha { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long Size { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Encoding { get; set; } = "utf-8";
    }

    /// <summary>
    /// Represents a Git tree entry
    /// </summary>
    public class GitTreeEntry
    {
        public string Path { get; set; } = string.Empty;
        public string Mode { get; set; } = "100644"; // File mode
        public string Type { get; set; } = "blob";
        public string Sha { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long Size { get; set; }
    }

    /// <summary>
    /// Represents a Git tree
    /// </summary>
    public class GitTree
    {
        public string Sha { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public List<GitTreeEntry> Tree { get; set; } = new();
        public bool Truncated { get; set; } = false;
    }

    /// <summary>
    /// Represents a Git commit
    /// </summary>
    public class GitCommit
    {
        public string Sha { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public GitCommitDetails Commit { get; set; } = new();
        public GitTree Tree { get; set; } = new();
        public List<GitCommit> Parents { get; set; } = new();
    }

    /// <summary>
    /// Git commit details
    /// </summary>
    public class GitCommitDetails
    {
        public string Message { get; set; } = string.Empty;
        public GitAuthor Author { get; set; } = new();
        public GitAuthor Committer { get; set; } = new();
        public GitTree Tree { get; set; } = new();
    }

    /// <summary>
    /// Git author/committer information
    /// </summary>
    public class GitAuthor
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Article metadata for Git operations
    /// </summary>
    public class ArticleGitMetadata
    {
        public int ArticleNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string UrlPath { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public Guid Id { get; set; }
        public DateTimeOffset LastModified { get; set; }
        public string Author { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Sha { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for creating/updating blobs
    /// </summary>
    public class CreateBlobRequest
    {
        public string Content { get; set; } = string.Empty;
        public string Encoding { get; set; } = "utf-8";
    }

    /// <summary>
    /// Request model for creating commits
    /// </summary>
    public class CreateCommitRequest
    {
        public string Message { get; set; } = string.Empty;
        public string Tree { get; set; } = string.Empty;
        public List<string> Parents { get; set; } = new();
        public GitAuthor Author { get; set; } = new();
        public GitAuthor Committer { get; set; } = new();
    }

    /// <summary>
    /// Request model for updating references
    /// </summary>
    public class UpdateRefRequest
    {
        public string Sha { get; set; } = string.Empty;
        public bool Force { get; set; } = false;
    }
}