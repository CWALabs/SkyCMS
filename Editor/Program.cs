// <copyright file="Program.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Sky.Editor.Boot;
using Hangfire;
using Sky.Editor.Services.Scheduling;
using Microsoft.Extensions.DependencyInjection;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

var isMultiTenantEditor = builder.Configuration.GetValue<bool?>("MultiTenantEditor") ?? false;
var versionNumber = Assembly.GetExecutingAssembly().GetName().Version.ToString();

// Register the ArticleVersionPublisher service
builder.Services.AddTransient<ArticleVersionPublisher>();

if (isMultiTenantEditor)
{
    System.Console.WriteLine($"Starting Cosmos CMS Editor in Multi-Tenant Mode (v.{versionNumber}).");
    var app = Cosmos.Editor.Boot.MultiTenant.BuildApp(builder);

    // Schedule the recurring job (runs every 5 minutes)
    RecurringJob.AddOrUpdate<ArticleVersionPublisher>(
        "article-version-publisher",
        service => service.ExecuteAsync(),
        "*/5 * * * *"); // Cron: every 5 minutes

    await app.RunAsync();
}
else
{
    System.Console.WriteLine($"Starting Cosmos CMS Editor in Single-Tenant Mode (v.{versionNumber}).");
    var app = SingleTenant.BuildApp(builder);

    // Schedule the recurring job (runs every 5 minutes)
    RecurringJob.AddOrUpdate<ArticleVersionPublisher>(
        "article-version-publisher",
        service => service.ExecuteAsync(),
        "*/5 * * * *"); // Cron: every 5 minutes

    await app.RunAsync();
}