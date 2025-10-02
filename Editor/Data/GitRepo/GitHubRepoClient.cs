// <copyright file="GitHubRepoClient.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Data.GitRepo;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Lightweight GitHub repository client using REST API v3.
/// Only implements the endpoints needed for file CRUD.
/// </summary>
public class GitHubRepoClient : IGitHubRepoClient
{
    /// <summary>
    /// Serializer options.
    /// </summary>
    private static readonly JsonSerializerOptions JsonSerializerOptions = new (JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient http;
    private readonly GitHubRepoOptions options;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubRepoClient"/> class.
    /// </summary>
    /// <param name="options">Github Options.</param>
    /// <param name="handler">Http message handler.</param>
    public GitHubRepoClient(GitHubRepoOptions options, HttpMessageHandler handler = null)
    {
        this.options = options;
        http = handler is null ? new HttpClient() : new HttpClient(handler);
        http.BaseAddress = new Uri(string.IsNullOrWhiteSpace(options.BaseUrl) ? "https://api.github.com" : options.BaseUrl.TrimEnd('/') + "/");
        http.DefaultRequestHeaders.UserAgent.ParseAdd(string.IsNullOrWhiteSpace(options.UserAgent) ? "SkyCMS-Editor" : options.UserAgent);
        if (!string.IsNullOrWhiteSpace(options.Token))
        {
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Token);
        }

        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        http.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<GitHubContentItem>> ListFilesAsync(string path = "", string @ref = null, CancellationToken ct = default)
    {
        path = path?.Trim('/') ?? string.Empty;
        var url = $"repos/{options.Owner}/{options.Repo}/contents/{Uri.EscapeDataString(path)}";
        if (!string.IsNullOrWhiteSpace(@ref))
        {
            url += $"?ref={Uri.EscapeDataString(@ref)}";
        }

        using var resp = await http.GetAsync(url, ct).ConfigureAwait(false);
        await EnsureSuccess(resp);
        var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        var items = await JsonSerializer.DeserializeAsync<List<GitHubContentItem>>(stream, JsonSerializerOptions, ct).ConfigureAwait(false);
        return items ?? new List<GitHubContentItem>();
    }

    /// <inheritdoc/>
    public async Task<GitHubFileGetResult> GetFileAsync(string path, string @ref = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        path = path.Trim('/');
        var url = $"repos/{options.Owner}/{options.Repo}/contents/{Uri.EscapeDataString(path)}";
        if (!string.IsNullOrWhiteSpace(@ref))
        {
            url += $"?ref={Uri.EscapeDataString(@ref)}";
        }

        using var resp = await http.GetAsync(url, ct).ConfigureAwait(false);
        await EnsureSuccess(resp);
        var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        var doc = await JsonSerializer.DeserializeAsync<GitHubFileContentResponse>(stream, JsonSerializerOptions, ct).ConfigureAwait(false)
                  ?? throw new InvalidOperationException("Empty response");

        if (!string.Equals(doc.Encoding, "base64", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException($"Unsupported encoding '{doc.Encoding}'");
        }

        var base64 = doc.ContentBase64.Replace("\n", string.Empty);
        var bytes = Convert.FromBase64String(base64);
        return new GitHubFileGetResult { Path = doc.Path, Sha = doc.Sha, Content = bytes };
    }

    /// <inheritdoc/>
    public async Task<GitHubWriteResult> CreateFileAsync(string path, byte[] content, string message, string branch = null, string authorName = null, string authorEmail = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        path = path.Trim('/');
        var url = $"repos/{options.Owner}/{options.Repo}/contents/{Uri.EscapeDataString(path)}";

        var body = new GitHubWriteRequest
        {
            Message = string.IsNullOrWhiteSpace(message) ? $"Create {path}" : message,
            ContentBase64 = Convert.ToBase64String(content ?? Array.Empty<byte>()),
            Branch = branch ?? options.DefaultBranch,
        };

        if (!string.IsNullOrWhiteSpace(authorName) && !string.IsNullOrWhiteSpace(authorEmail))
        {
            body.Author = new GitHubCommitIdentity { Name = authorName!, Email = authorEmail! };
            body.Committer = body.Author;
        }

        using var resp = await http.PutAsync(url, Json(body), ct).ConfigureAwait(false);
        await EnsureSuccess(resp);
        var wr = await DeserializeWriteResponse(resp, ct).ConfigureAwait(false);
        return wr;
    }

    /// <inheritdoc/>
    public async Task<GitHubWriteResult> UpdateFileAsync(string path, string sha, byte[] content, string message, string branch = null, string authorName = null, string authorEmail = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (string.IsNullOrWhiteSpace(sha))
        {
            throw new ArgumentNullException(nameof(sha));
        }

        path = path.Trim('/');
        var url = $"repos/{options.Owner}/{options.Repo}/contents/{Uri.EscapeDataString(path)}";

        var body = new GitHubWriteRequest
        {
            Message = string.IsNullOrWhiteSpace(message) ? $"Update {path}" : message,
            ContentBase64 = Convert.ToBase64String(content ?? Array.Empty<byte>()),
            Sha = sha,
            Branch = branch ?? options.DefaultBranch,
        };
        if (!string.IsNullOrWhiteSpace(authorName) && !string.IsNullOrWhiteSpace(authorEmail))
        {
            body.Author = new GitHubCommitIdentity { Name = authorName!, Email = authorEmail! };
            body.Committer = body.Author;
        }

        using var resp = await http.PutAsync(url, Json(body), ct).ConfigureAwait(false);
        await EnsureSuccess(resp);
        var wr = await DeserializeWriteResponse(resp, ct).ConfigureAwait(false);
        return wr;
    }

    /// <inheritdoc/>
    public async Task DeleteFileAsync(string path, string sha, string message, string branch = null, string authorName = null, string authorEmail = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (string.IsNullOrWhiteSpace(sha))
        {
            throw new ArgumentNullException(nameof(sha));
        }

        path = path.Trim('/');
        var url = $"repos/{options.Owner}/{options.Repo}/contents/{Uri.EscapeDataString(path)}";

        var body = new GitHubWriteRequest
        {
            Message = string.IsNullOrWhiteSpace(message) ? $"Delete {path}" : message,
            Sha = sha,
            Branch = branch ?? options.DefaultBranch,
        };
        if (!string.IsNullOrWhiteSpace(authorName) && !string.IsNullOrWhiteSpace(authorEmail))
        {
            body.Author = new GitHubCommitIdentity { Name = authorName!, Email = authorEmail! };
            body.Committer = body.Author;
        }

        using var req = new HttpRequestMessage(HttpMethod.Delete, url) { Content = Json(body) };
        using var resp = await http.SendAsync(req, ct).ConfigureAwait(false);
        await EnsureSuccess(resp);
    }

    /// <summary>
    ///  Ensure the HTTP response indicates success, otherwise throw with details.
    /// </summary>
    /// <param name="resp">HTTP response message.</param>
    /// <returns>Task.</returns>
    /// <exception cref="HttpRequestException">Request exception.</exception>
    private static async Task EnsureSuccess(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode) return;
        var text = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
        throw new HttpRequestException($"GitHub API error {(int)resp.StatusCode} {resp.ReasonPhrase}: {text}");
    }

    private static StringContent Json(object payload)
        => new StringContent(System.Text.Json.JsonSerializer.Serialize(payload, JsonSerializerOptions), Encoding.UTF8, "application/json");

    /// <summary>
    ///  Deserialize the write response.
    /// </summary>
    /// <param name="resp">HTTP response message.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Write result.</returns>
    private static async Task<GitHubWriteResult> DeserializeWriteResponse(HttpResponseMessage resp, CancellationToken ct)
    {
        var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        var doc = await JsonSerializer.DeserializeAsync<GitHubWriteResponse>(stream, JsonSerializerOptions, ct).ConfigureAwait(false)
                  ?? new GitHubWriteResponse();
        return new GitHubWriteResult
        {
            Path = doc.Content?.Path ?? string.Empty,
            Sha = doc.Content?.Sha ?? string.Empty,
            CommitSha = doc.Commit?.Sha ?? string.Empty,
        };
    }
}
