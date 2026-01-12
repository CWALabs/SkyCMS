// <copyright file="Mediator.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Features.Shared;

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Simple mediator implementation using service provider for handler resolution.
/// </summary>
/// <remarks>
/// This implementation uses reflection and the dependency injection container to dynamically
/// resolve and invoke the appropriate command or query handlers at runtime.
/// The mediator follows the CQRS pattern by providing separate methods for commands and queries.
/// </remarks>
public class Mediator : IMediator
{
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve command and query handlers.</param>
    public Mediator(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public async Task<TResult> SendAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));

        var handler = serviceProvider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod(nameof(ICommandHandler<ICommand<TResult>, TResult>.HandleAsync));

        if (method == null)
        {
            throw new InvalidOperationException($"Handler method not found for {commandType.Name}");
        }

        var result = method.Invoke(handler, [command, cancellationToken]);

        if (result is Task<TResult> task)
        {
            return await task;
        }

        throw new InvalidOperationException($"Handler did not return expected type for {commandType.Name}");
    }

    /// <inheritdoc/>
    public async Task<TResult> QueryAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var queryType = query.GetType();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));

        var handler = serviceProvider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod(nameof(IQueryHandler<IQuery<TResult>, TResult>.HandleAsync));

        if (method == null)
        {
            throw new InvalidOperationException($"Handler method not found for {queryType.Name}");
        }

        var result = method.Invoke(handler, [query, cancellationToken]);

        if (result is Task<TResult> task)
        {
            return await task;
        }

        throw new InvalidOperationException($"Handler did not return expected type for {queryType.Name}");
    }
}
