// <copyright file="CommandResult.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Features.Shared
{
    using System.Collections.Generic;

    /// <summary>
    /// Base result for command execution with success/failure status.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class provides a standard result pattern for command handlers in the CQRS architecture.
    /// It encapsulates the success or failure state of a command execution along with optional error information.
    /// </para>
    /// <para>
    /// Use <see cref="CommandResult{T}"/> when the command needs to return data upon successful execution.
    /// </para>
    /// </remarks>
    public class CommandResult
    {
        /// <summary>
        /// Gets a value indicating whether the command execution was successful.
        /// </summary>
        /// <value>
        /// <c>true</c> if the command executed successfully; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccess { get; init; }

        /// <summary>
        /// Gets the error message when the command execution fails.
        /// </summary>
        /// <value>
        /// A string containing the error message, or <c>null</c> if the command succeeded or validation errors are used instead.
        /// </value>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Gets the validation errors when the command execution fails due to validation issues.
        /// </summary>
        /// <value>
        /// A dictionary where the key is the property name and the value is an array of error messages for that property,
        /// or <c>null</c> if the command succeeded or a single error message is used instead.
        /// </value>
        public Dictionary<string, string[]>? Errors { get; init; }

        /// <summary>
        /// Creates a successful command result.
        /// </summary>
        /// <returns>A <see cref="CommandResult"/> instance with <see cref="IsSuccess"/> set to <c>true</c>.</returns>
        /// <example>
        /// <code>
        /// return CommandResult.Success();
        /// </code>
        /// </example>
        public static CommandResult Success() => new() { IsSuccess = true };

        /// <summary>
        /// Creates a failed command result with a single error message.
        /// </summary>
        /// <param name="errorMessage">The error message describing why the command failed.</param>
        /// <returns>A <see cref="CommandResult"/> instance with <see cref="IsSuccess"/> set to <c>false</c> and the specified error message.</returns>
        /// <example>
        /// <code>
        /// return CommandResult.Failure("Article title cannot be empty");
        /// </code>
        /// </example>
        public static CommandResult Failure(string errorMessage) =>
            new() { IsSuccess = false, ErrorMessage = errorMessage };

        /// <summary>
        /// Creates a failed command result with validation errors.
        /// </summary>
        /// <param name="errors">A dictionary of validation errors where the key is the property name and the value is an array of error messages.</param>
        /// <returns>A <see cref="CommandResult"/> instance with <see cref="IsSuccess"/> set to <c>false</c> and the specified validation errors.</returns>
        /// <example>
        /// <code>
        /// var errors = new Dictionary&lt;string, string[]&gt;
        /// {
        ///     { "Title", new[] { "Title is required", "Title must be less than 200 characters" } },
        ///     { "Content", new[] { "Content is required" } }
        /// };
        /// return CommandResult.Failure(errors);
        /// </code>
        /// </example>
        public static CommandResult Failure(Dictionary<string, string[]> errors) =>
            new() { IsSuccess = false, Errors = errors };
    }

    /// <summary>
    /// Result for command execution that returns data upon successful execution.
    /// </summary>
    /// <typeparam name="T">The type of data returned by the command.</typeparam>
    /// <remarks>
    /// <para>
    /// This generic result type extends <see cref="CommandResult"/> to include a <see cref="Data"/> property
    /// that contains the result of a successful command execution.
    /// </para>
    /// <para>
    /// This is commonly used in CQRS command handlers that need to return information about the created or modified entity,
    /// such as returning an <c>ArticleViewModel</c> after creating or updating an article.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class CreateArticleCommand : ICommand&lt;CommandResult&lt;ArticleViewModel&gt;&gt;
    /// {
    ///     public string Title { get; set; }
    ///     public string Content { get; set; }
    /// }
    /// 
    /// // In the handler:
    /// var article = new Article { Title = command.Title, Content = command.Content };
    /// await _db.Articles.AddAsync(article);
    /// await _db.SaveChangesAsync();
    /// 
    /// var viewModel = new ArticleViewModel { Id = article.Id, Title = article.Title };
    /// return CommandResult&lt;ArticleViewModel&gt;.Success(viewModel);
    /// </code>
    /// </example>
    public class CommandResult<T> : CommandResult
    {
        /// <summary>
        /// Gets the data returned by the successful command execution.
        /// </summary>
        /// <value>
        /// The data of type <typeparamref name="T"/>, or <c>null</c> if the command failed or did not return data.
        /// </value>
        public T? Data { get; init; }

        /// <summary>
        /// Creates a successful command result with data.
        /// </summary>
        /// <param name="data">The data to return from the successful command execution.</param>
        /// <returns>A <see cref="CommandResult{T}"/> instance with <see cref="CommandResult.IsSuccess"/> set to <c>true</c> and the specified data.</returns>
        /// <example>
        /// <code>
        /// var article = new ArticleViewModel { Id = 1, Title = "My Article" };
        /// return CommandResult&lt;ArticleViewModel&gt;.Success(article);
        /// </code>
        /// </example>
        public static CommandResult<T> Success(T data) =>
            new() { IsSuccess = true, Data = data };

        /// <summary>
        /// Creates a failed command result with a single error message.
        /// </summary>
        /// <param name="errorMessage">The error message describing why the command failed.</param>
        /// <returns>A <see cref="CommandResult{T}"/> instance with <see cref="CommandResult.IsSuccess"/> set to <c>false</c> and the specified error message.</returns>
        /// <example>
        /// <code>
        /// return CommandResult&lt;ArticleViewModel&gt;.Failure("Article not found");
        /// </code>
        /// </example>
        public new static CommandResult<T> Failure(string errorMessage) =>
            new() { IsSuccess = false, ErrorMessage = errorMessage };

        /// <summary>
        /// Creates a failed command result with validation errors.
        /// </summary>
        /// <param name="errors">A dictionary of validation errors where the key is the property name and the value is an array of error messages.</param>
        /// <returns>A <see cref="CommandResult{T}"/> instance with <see cref="CommandResult.IsSuccess"/> set to <c>false</c> and the specified validation errors.</returns>
        /// <example>
        /// <code>
        /// var errors = new Dictionary&lt;string, string[]&gt;
        /// {
        ///     { "Title", new[] { "Title is required" } }
        /// };
        /// return CommandResult&lt;ArticleViewModel&gt;.Failure(errors);
        /// </code>
        /// </example>
        public new static CommandResult<T> Failure(Dictionary<string, string[]> errors) =>
            new() { IsSuccess = false, Errors = errors };
    }
}