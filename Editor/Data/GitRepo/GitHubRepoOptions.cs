namespace Sky.Editor.Data.GitRepo;

/// <summary>
/// Options for connecting to a GitHub repository.
/// </summary>
public class GitHubRepoOptions
{
    /// <summary>
    /// GitHub API base URL. Defaults to https://api.github.com.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.github.com";

    /// <summary>
    /// Repository owner or organization name.
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Repository name.
    /// </summary>
    public string Repo { get; set; } = string.Empty;

    /// <summary>
    /// Default branch to use for operations if not specified.
    /// </summary>
    public string DefaultBranch { get; set; } = "main";

    /// <summary>
    /// Personal Access Token (classic) or a fine-grained token with repo contents permissions.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// A descriptive user agent to satisfy GitHub API requirements.
    /// </summary>
    public string UserAgent { get; set; } = "SkyCMS-Editor";
}
