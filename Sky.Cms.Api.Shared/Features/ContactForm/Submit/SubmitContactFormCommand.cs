// <copyright file="SubmitContactFormCommand.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Api.Shared.Features.ContactForm.Submit;

using Cosmos.Common.Features.Shared;
using Sky.Cms.Api.Shared.Models;

/// <summary>
/// Command for submitting a contact form.
/// </summary>
public class SubmitContactFormCommand : ICommand<CommandResult<ContactFormResponse>>
{
    /// <summary>
    /// Gets or sets the contact form request.
    /// </summary>
    public ContactFormRequest Request { get; set; } = null!;

    /// <summary>
    /// Gets or sets the remote IP address of the submitter.
    /// </summary>
    public string RemoteIpAddress { get; set; } = null!;
}