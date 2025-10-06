// <copyright file="DomainEventRegistrationExtensions.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Domain.Events
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// DI helper extensions to register domain event handlers and dispatcher.
    /// </summary>
    public static class DomainEventRegistrationExtensions
    {
        /// <summary>
        /// Registers the domain event dispatcher and supplied handlers.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="parallel">Whether to dispatch handlers for a single event in parallel.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddDomainEvents(this IServiceCollection services, bool parallel = false)
        {
            services.AddSingleton<IDomainEventDispatcher>(sp =>
                new DomainEventDispatcher(
                    handlerInterface =>
                        sp.GetServices(handlerInterface),
                    parallel));

            // Example handler registrations. Remove or adjust as needed.
            services.AddScoped<IDomainEventHandler<ArticlePublishedEvent>, Handlers.ArticlePublishedEventHandler>();
            services.AddScoped<IDomainEventHandler<TitleChangedEvent>, Handlers.TitleChangedEventHandler>();
            services.AddScoped<IDomainEventHandler<RedirectCreatedEvent>, Handlers.RedirectCreatedEventHandler>();
            services.AddScoped<IDomainEventHandler<CatalogUpdatedEvent>, Handlers.CatalogUpdatedEventHandler>();

            // Optional open generic logging handler (lowest priority).
            services.AddScoped(typeof(IDomainEventHandler<>), typeof(Handlers.CompositeLoggingEventHandler<>));

            return services;
        }
    }
}