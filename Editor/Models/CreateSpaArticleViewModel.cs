// <copyright file="CreateSpaArticleViewModel.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Models;

using System;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// View model for creating a new SPA article.
/// </summary>
public class CreateSpaArticleViewModel
{
    /// <summary>
    /// Gets or sets the article ID (generated on creation).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the friendly title of the SPA.
    /// </summary>
    [Required]
    [MaxLength(254)]
    [Display(Name = "Application Title")]
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the URL path for the SPA (e.g., "/my-react-app").
    /// </summary>
    [Required]
    [MaxLength(255)]
    [RegularExpression(@"^/[a-z0-9\-]+$", ErrorMessage = "URL must start with / and contain only lowercase letters, numbers, and hyphens")]
    [Display(Name = "URL Path")]
    public string UrlPath { get; set; }

    /// <summary>
    /// Gets or sets the deployment key (password) - shown only once.
    /// </summary>
    public string DeploymentKey { get; set; }

    /// <summary>
    /// Gets or sets the webhook secret - shown only once.
    /// </summary>
    public string WebhookSecret { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether secrets are being displayed (creation confirmation).
    /// </summary>
    public bool ShowingSecrets { get; set; }
}