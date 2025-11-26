// <copyright file="IMediator.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Features.Shared
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Mediator for dispatching commands and queries to their handlers using the CQRS pattern.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface defines the contract for a mediator implementation that separates commands (write operations)
    /// from queries (read operations) following Command Query Responsibility Segregation (CQRS) principles.
    /// </para>
    /// <para>
    /// The mediator pattern helps decouple the sender of a request from the handler that processes it,
    /// enabling better separation of concerns and testability.
    /// </para>
    /// </remarks>
    public interface IMediator
    {
        /// <summary>
        /// Sends a command to its registered handler for processing.
        /// </summary>
        /// <typeparam name="TResult">The type of result returned by the command handler.</typeparam>
        /// <param name="command">The command instance to be processed by its handler.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the result from the command handler.</returns>
        /// <remarks>
        /// <para>
        /// Commands typically represent write operations that modify application state (e.g., create, update, delete operations).
        /// The mediator will locate the appropriate <see cref="ICommandHandler{TCommand, TResult}"/> and invoke its handler method.
        /// </para>
        /// <para>
        /// Example usage:
        /// <code>
        /// var command = new SaveArticleCommand { Title = "New Article", Content = "..." };
        /// var result = await mediator.SendAsync(command);
        /// </code>
        /// </para>
        /// </remarks>
        Task<TResult> SendAsync<TResult>(
            ICommand<TResult> command,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a query to its registered handler for processing.
        /// </summary>
        /// <typeparam name="TResult">The type of result returned by the query handler.</typeparam>
        /// <param name="query">The query instance to be processed by its handler.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the result from the query handler.</returns>
        /// <remarks>
        /// <para>
        /// Queries typically represent read operations that retrieve data without modifying application state.
        /// The mediator will locate the appropriate <see cref="IQueryHandler{TQuery, TResult}"/> and invoke its handler method.
        /// </para>
        /// <para>
        /// Example usage:
        /// <code>
        /// var query = new GetArticleByIdQuery { ArticleId = 123 };
        /// var result = await mediator.QueryAsync(query);
        /// </code>
        /// </para>
        /// </remarks>
        Task<TResult> QueryAsync<TResult>(
            IQuery<TResult> query,
            CancellationToken cancellationToken = default);
    }
}