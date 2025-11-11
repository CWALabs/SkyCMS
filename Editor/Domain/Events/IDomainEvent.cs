// <copyright file="IDomainEvent.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Domain.Events
{
    using System;

    /// <summary>
    /// Represents a domain event with an occurrence timestamp in UTC.
    /// Implementations capture something that happened in the domain model
    /// and may be dispatched to zero or more handlers.
    /// </summary>
    /// <remarks>
    /// Domain events should be immutable; capture all necessary data at creation time.
    /// </remarks>
    public interface IDomainEvent
    {
        /// <summary>
        /// Gets the UTC timestamp when the event instance was created.
        /// </summary>
        DateTimeOffset OccurredOn { get; }
    }

    /// <summary>
    /// Base implementation of <see cref="IDomainEvent"/> providing an automatic UTC occurrence timestamp.
    /// </summary>
    public abstract class DomainEventBase : IDomainEvent
    {
        /// <summary>
        /// Gets the UTC timestamp when the event was instantiated.
        /// </summary>
        public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Event raised when an article version is published.
    /// </summary>
    public sealed class ArticlePublishedEvent : DomainEventBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArticlePublishedEvent"/> class.
        /// </summary>
        /// <param name="articleNumber">Logical (stable) article number.</param>
        /// <param name="articleId">Concrete article version identifier.</param>
        public ArticlePublishedEvent(int articleNumber, Guid articleId)
        {
            ArticleNumber = articleNumber;
            ArticleId = articleId;
        }

        /// <summary>
        /// Gets the logical article number associated with the publication.
        /// </summary>
        public int ArticleNumber { get; }

        /// <summary>
        /// Gets the unique identifier of the published article entity (specific version).
        /// </summary>
        public Guid ArticleId { get; }
    }

    /// <summary>
    /// Event raised when an article title changes (potentially impacting its slug).
    /// </summary>
    public sealed class TitleChangedEvent : DomainEventBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TitleChangedEvent"/> class.
        /// </summary>
        /// <param name="articleNumber">Logical article number.</param>
        /// <param name="oldTitle">Title before the change.</param>
        /// <param name="newTitle">Title after the change.</param>
        public TitleChangedEvent(int articleNumber, string oldTitle, string newTitle)
        {
            ArticleNumber = articleNumber;
            OldTitle = oldTitle;
            NewTitle = newTitle;
        }

        /// <summary>
        /// Gets the logical article number whose title changed.
        /// </summary>
        public int ArticleNumber { get; }

        /// <summary>
        /// Gets the previous title value.
        /// </summary>
        public string OldTitle { get; }

        /// <summary>
        /// Gets the new title value.
        /// </summary>
        public string NewTitle { get; }
    }

    /// <summary>
    /// Event raised when a redirect is created mapping one slug to another.
    /// </summary>
    public sealed class RedirectCreatedEvent : DomainEventBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectCreatedEvent"/> class.
        /// </summary>
        /// <param name="fromSlug">Original (source) slug.</param>
        /// <param name="toSlug">Destination (target) slug.</param>
        public RedirectCreatedEvent(string fromSlug, string toSlug)
        {
            FromSlug = fromSlug;
            ToSlug = toSlug;
        }

        /// <summary>
        /// Gets the original slug that will redirect.
        /// </summary>
        public string FromSlug { get; }

        /// <summary>
        /// Gets the target slug receiving traffic.
        /// </summary>
        public string ToSlug { get; }
    }

    /// <summary>
    /// Event raised when an article's catalog entry is created or updated.
    /// </summary>
    public sealed class CatalogUpdatedEvent : DomainEventBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CatalogUpdatedEvent"/> class.
        /// </summary>
        /// <param name="articleNumber">Logical article number whose catalog data changed.</param>
        public CatalogUpdatedEvent(int articleNumber) => ArticleNumber = articleNumber;

        /// <summary>
        /// Gets the logical article number linked to the catalog update.
        /// </summary>
        public int ArticleNumber { get; }
    }
}