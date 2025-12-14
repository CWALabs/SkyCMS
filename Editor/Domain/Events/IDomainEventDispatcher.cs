// <copyright file="IDomainEventDispatcher.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Domain.Events
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /*
        PSEUDOCODE / PLAN (documentation update only):
        - Add XML documentation to interface describing purpose, usage expectations, ordering, error behavior.
        - Add docs to each overload distinguishing legacy (no CancellationToken) vs enhanced (cancellable).
        - Clarify that collection overload should be treated atomically in terms of intent (best-effort sequential dispatch).
        - Mention idempotency expectations belong to handlers, not dispatcher.
        - Note that cancellation only honored before dispatch of an individual handler invocation (implementation detail guidance).
        - Keep existing method signatures unchanged.
    */

    /// <summary>
    /// Dispatches domain events (<see cref="IDomainEvent"/>) to their corresponding handlers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This abstraction decouples the domain model from infrastructure concerns (e.g., dependency injection,
    /// handler discovery, transaction boundaries, and asynchronous fan-out).
    /// </para>
    /// <para>
    /// Implementations SHOULD:
    /// <list type="bullet">
    /// <item>Preserve the ordering of events passed in an <see cref="IEnumerable{T}"/> (FIFO) when invoking handlers.</item>
    /// <item>Dispatch each event to zero or more handlers discovered at runtime.</item>
    /// <item>Surface the first thrown exception (fail fast) unless explicitly designed for resilient / best-effort dispatch.</item>
    /// <item>Avoid swallowing exceptions silently; aggregate if necessary when multiple handlers fail.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Implementations MAY:
    /// <list type="bullet">
    /// <item>Batch or pipeline handler execution for performance, provided observable ordering per event is preserved.</item>
    /// <item>Perform parallel handler invocation for a single event if ordering between handlers is not part of the contract.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Idempotency is the responsibility of individual handlers — the dispatcher does not guarantee at-most-once delivery
    /// in the presence of retries or failures.
    /// </para>
    /// <para>
    /// Cancellation tokens (on the overloads that accept them) should be honored best-effort before dispatch begins
    /// for each event; once a handler invocation has started, cooperative cancellation depends on the handler.
    /// </para>
    /// </remarks>
    /// <seealso cref="IDomainEvent"/>
    public interface IDomainEventDispatcher
    {
        /// <summary>
        /// Dispatches a single domain event to all registered handlers.
        /// </summary>
        /// <param name="event">The domain event instance to dispatch.</param>
        /// <returns>A task that completes when all handlers have finished processing.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="event"/> is <c>null</c>.</exception>
        /// <remarks>
        /// Legacy / convenience overload retained for backward compatibility. Equivalent to calling the
        /// cancellable overload with <see cref="CancellationToken.None"/>.
        /// </remarks>
        Task DispatchAsync(IDomainEvent @event);

        /// <summary>
        /// Dispatches a sequence of domain events to their handlers in the order provided.
        /// </summary>
        /// <param name="events">The ordered collection of domain events to dispatch.</param>
        /// <returns>A task that completes when all events have been dispatched and their handlers have run.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="events"/> is <c>null</c>.</exception>
        /// <remarks>
        /// Each event is dispatched sequentially (FIFO) unless an implementation documents otherwise.
        /// Fail-fast behavior (throwing on the first failure) is recommended unless resiliency / aggregation is required.
        /// </remarks>
        Task DispatchAsync(IEnumerable<IDomainEvent> events);

        /// <summary>
        /// Dispatches a single domain event to all registered handlers, honoring cancellation.
        /// </summary>
        /// <param name="event">The domain event instance to dispatch.</param>
        /// <param name="cancellationToken">A token used to request cancellation before handler invocation begins.</param>
        /// <returns>A task that completes when all handlers have finished or cancellation is requested.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="event"/> is <c>null</c>.</exception>
        /// <remarks>
        /// Implementations should check <paramref name="cancellationToken"/> prior to each handler invocation.
        /// If cancellation is requested mid-dispatch, remaining handlers SHOULD NOT be invoked.
        /// </remarks>
        Task DispatchAsync(IDomainEvent @event, CancellationToken cancellationToken);

        /// <summary>
        /// Dispatches a sequence of domain events to their handlers, honoring cancellation.
        /// </summary>
        /// <param name="events">The ordered collection of domain events to dispatch.</param>
        /// <param name="cancellationToken">A token used to request cancellation before dispatch of each event.</param>
        /// <returns>A task that completes when all events (and their handlers) have been processed or cancellation is requested.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="events"/> is <c>null</c>.</exception>
        /// <remarks>
        /// Implementations should evaluate <paramref name="cancellationToken"/> before dispatching each event.
        /// Partially processed batches are possible if cancellation occurs mid-sequence.
        /// </remarks>
        Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken);
    }
}
