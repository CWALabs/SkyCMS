// <copyright file="Mediator.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Features.Shared
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Simple mediator implementation using service provider for handler resolution.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This implementation of <see cref="IMediator"/> uses reflection and the dependency injection container
    /// (<see cref="IServiceProvider"/>) to dynamically resolve and invoke the appropriate command or query handlers
    /// at runtime.
    /// </para>
    /// <para>
    /// The mediator follows the CQRS (Command Query Responsibility Segregation) pattern by providing separate
    /// methods for commands (<see cref="SendAsync{TResult}"/>) and queries (<see cref="QueryAsync{TResult}"/>).
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> This class is thread-safe as long as the underlying <see cref="IServiceProvider"/>
    /// is thread-safe (which is the default behavior in ASP.NET Core applications).
    /// </para>
    /// </remarks>
    public class Mediator : IMediator
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mediator"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve command and query handlers.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
        public Mediator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Sends a command to its registered handler for processing.
        /// </summary>
        /// <typeparam name="TResult">The type of result returned by the command handler.</typeparam>
        /// <param name="command">The command instance to be processed by its handler.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the result from the command handler.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when:
        /// <list type="bullet">
        ///   <item>No handler is registered for the command type in the service provider.</item>
        ///   <item>The handler does not have the expected <c>HandleAsync</c> method.</item>
        ///   <item>The handler does not return the expected <see cref="Task{TResult}"/> type.</item>
        /// </list>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method uses reflection to dynamically construct the handler type (<see cref="ICommandHandler{TCommand, TResult}"/>)
        /// based on the command's runtime type, resolves it from the DI container, and invokes its <c>HandleAsync</c> method.
        /// </para>
        /// <para>
        /// <strong>Performance Consideration:</strong> The use of reflection incurs a performance cost. For high-throughput
        /// scenarios, consider using source generators or compiled expression trees to cache handler invocations.
        /// </para>
        /// <para>
        /// Example usage:
        /// <code>
        /// var command = new SaveArticleCommand 
        /// { 
        ///     Title = "New Article", 
        ///     Content = "Article content", 
        ///     UserId = currentUserId 
        /// };
        /// var result = await mediator.SendAsync(command);
        /// if (result.IsSuccess)
        /// {
        ///     // Handle success
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        public async Task<TResult> SendAsync<TResult>(
            ICommand<TResult> command,
            CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var commandType = command.GetType();
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));

            var handler = serviceProvider.GetRequiredService(handlerType);
            var method = handlerType.GetMethod(nameof(ICommandHandler<ICommand<TResult>, TResult>.HandleAsync));

            if (method == null)
            {
                throw new InvalidOperationException($"Handler method not found for {commandType.Name}");
            }

            var result = method.Invoke(handler, new object[] { command, cancellationToken });

            if (result is Task<TResult> task)
            {
                return await task;
            }

            throw new InvalidOperationException($"Handler did not return expected type for {commandType.Name}");
        }

        /// <summary>
        /// Sends a query to its registered handler for processing.
        /// </summary>
        /// <typeparam name="TResult">The type of result returned by the query handler.</typeparam>
        /// <param name="query">The query instance to be processed by its handler.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the result from the query handler.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="query"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when:
        /// <list type="bullet">
        ///   <item>No handler is registered for the query type in the service provider.</item>
        ///   <item>The handler does not have the expected <c>HandleAsync</c> method.</item>
        ///   <item>The handler does not return the expected <see cref="Task{TResult}"/> type.</item>
        /// </list>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method uses reflection to dynamically construct the handler type (<see cref="IQueryHandler{TQuery, TResult}"/>)
        /// based on the query's runtime type, resolves it from the DI container, and invokes its <c>HandleAsync</c> method.
        /// </para>
        /// <para>
        /// <strong>Performance Consideration:</strong> The use of reflection incurs a performance cost. For high-throughput
        /// scenarios, consider using source generators or compiled expression trees to cache handler invocations.
        /// </para>
        /// <para>
        /// Example usage:
        /// <code>
        /// var query = new GetArticleByIdQuery { ArticleId = 123 };
        /// var article = await mediator.QueryAsync(query);
        /// if (article != null)
        /// {
        ///     // Process article
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        public async Task<TResult> QueryAsync<TResult>(
            IQuery<TResult> query,
            CancellationToken cancellationToken = default)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var queryType = query.GetType();
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));

            var handler = serviceProvider.GetRequiredService(handlerType);
            var method = handlerType.GetMethod(nameof(IQueryHandler<IQuery<TResult>, TResult>.HandleAsync));

            if (method == null)
            {
                throw new InvalidOperationException($"Handler method not found for {queryType.Name}");
            }

            var result = method.Invoke(handler, new object[] { query, cancellationToken });

            if (result is Task<TResult> task)
            {
                return await task;
            }

            throw new InvalidOperationException($"Handler did not return expected type for {queryType.Name}");
        }
    }
}