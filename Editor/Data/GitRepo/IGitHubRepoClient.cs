// <copyright file="IGitHubRepoClient.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Data.GitRepo;

using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

/// <summary>
/// Minimal GitHub repo client for file content operations.
/// </summary>
public interface IGitHubRepoClient
{
    /// <summary>
    /// List files in a directory.
    /// </summary>
    /// <param name="path">Directory path.</param>
    /// <param name="ref">Optional branch or tag reference.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of files in the directory.</returns>
    Task<IReadOnlyList<GitHubContentItem>> ListFilesAsync(string path = "", string? @ref = null, CancellationToken ct = default);

    /// <summary>
    ///  Get a file's content and SHA.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <param name="ref">Optional branch or tag reference.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>File content and SHA.</returns>
    Task<GitHubFileGetResult> GetFileAsync(string path, string? @ref = null, CancellationToken ct = default);

    /// <summary>
    ///  Create a new file in the repository.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <param name="content">File content.</param>
    /// <param name="message">Commit message.</param>
    /// <param name="branch">Optional branch name.</param>
    /// <param name="authorName">Optional author name.</param>
    /// <param name="authorEmail">Optional author email.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Write result.</returns>
    Task<GitHubWriteResult> CreateFileAsync(string path, byte[] content, string message, string? branch = null, string? authorName = null, string? authorEmail = null, CancellationToken ct = default);

    /// <summary>
    ///  Update an existing file in the repository.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <param name="sha">File SHA.</param>
    /// <param name="content">File content.</param>
    /// <param name="message">Commit message.</param>
    /// <param name="branch">Optional branch name.</param>
    /// <param name="authorName">Optional author name.</param>
    /// <param name="authorEmail">Optional author email.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Write result.</returns>
    Task<GitHubWriteResult> UpdateFileAsync(string path, string sha, byte[] content, string message, string? branch = null, string? authorName = null, string? authorEmail = null, CancellationToken ct = default);

    /// <summary>
    ///  Delete a file from the repository.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <param name="sha">File SHA.</param>
    /// <param name="message">Commit message.</param>
    /// <param name="branch">Optional branch name.</param>
    /// <param name="authorName">Optional author name.</param>
    /// <param name="authorEmail">Optional author email.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task.</returns>
    Task DeleteFileAsync(string path, string sha, string message, string? branch = null, string? authorName = null, string? authorEmail = null, CancellationToken ct = default);
}
