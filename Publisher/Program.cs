// <copyright file="Program.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

var builder = WebApplication.CreateBuilder(args);

var isStaticWebsite =
    builder.Configuration.GetValue<bool?>("CosmosStaticWebPages") ?? false;

if (isStaticWebsite)
{
    await Cosmos.Publisher.Boot.StaticWebsiteProxy.Boot(builder);
}
else
{
    await Cosmos.Publisher.Boot.DynamicPublisherWebsite.Boot(builder);
}
