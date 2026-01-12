// <copyright file="IQuery.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Features.Shared;

/// <summary>
/// Marker interface for queries in the CQRS pattern.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query handler.</typeparam>
/// <remarks>
/// Queries represent read operations that retrieve data without modifying application state.
/// They are processed by implementations of <see cref="IQueryHandler{TQuery, TResult}"/>.
/// </remarks>
public interface IQuery<TResult>
{
}
