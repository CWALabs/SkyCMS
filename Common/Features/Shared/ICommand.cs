// <copyright file="ICommand.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Features.Shared;

/// <summary>
/// Marker interface for commands in the CQRS pattern.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the command handler.</typeparam>
/// <remarks>
/// Commands represent write operations that modify application state (e.g., create, update, delete).
/// They are processed by implementations of <see cref="ICommandHandler{TCommand, TResult}"/>.
/// </remarks>
public interface ICommand<TResult>
{
}
