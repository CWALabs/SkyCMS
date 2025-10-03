using System.Text.Json.Serialization;

namespace Sky.Editor.Data.GitRepo;

public class GitHubContentItem
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("path")] public string Path { get; set; } = string.Empty;
    [JsonPropertyName("sha")] public string Sha { get; set; } = string.Empty;
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty; // file, dir, symlink, submodule
    [JsonPropertyName("size")] public long Size { get; set; }
    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
    [JsonPropertyName("download_url")] public string? DownloadUrl { get; set; }
}

public class GitHubFileGetResult
{
    public string Path { get; set; } = string.Empty;
    public string Sha { get; set; } = string.Empty;
    public byte[] Content { get; set; } = System.Array.Empty<byte>();
}

internal class GitHubFileContentResponse
{
    [JsonPropertyName("sha")] public string Sha { get; set; } = string.Empty;
    [JsonPropertyName("content")] public string ContentBase64 { get; set; } = string.Empty; // base64 with newlines
    [JsonPropertyName("encoding")] public string Encoding { get; set; } = "base64";
    [JsonPropertyName("path")] public string Path { get; set; } = string.Empty;
}

internal class GitHubWriteRequest
{
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("content")] public string? ContentBase64 { get; set; }
    [JsonPropertyName("sha")] public string? Sha { get; set; }
    [JsonPropertyName("branch")] public string? Branch { get; set; }
    [JsonPropertyName("committer")] public GitHubCommitIdentity? Committer { get; set; }
    [JsonPropertyName("author")] public GitHubCommitIdentity? Author { get; set; }
}

internal class GitHubCommitIdentity
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
}

public class GitHubWriteResult
{
    public string Path { get; set; } = string.Empty;
    public string Sha { get; set; } = string.Empty;
    public string CommitSha { get; set; } = string.Empty;
}

internal class GitHubWriteResponse
{
    [JsonPropertyName("content")] public GitHubContentItem? Content { get; set; }
    [JsonPropertyName("commit")] public GitHubWriteCommit Commit { get; set; } = new();
}

internal class GitHubWriteCommit
{
    [JsonPropertyName("sha")] public string Sha { get; set; } = string.Empty;
}
