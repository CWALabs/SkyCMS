# Services Documentation

This document provides an overview of all service classes contained in the `Editor\Services` folder and its subdirectories.

## Table of Contents

- [Core Services](#core-services)
- [Author Services](#author-services)
- [Blog Publishing Services](#blog-publishing-services)
- [Catalog Services](#catalog-services)
- [CDN Services](#cdn-services)
- [HTML Services](#html-services)
- [Publishing Services](#publishing-services)
- [Redirect Services](#redirect-services)
- [Reserved Paths Services](#reserved-paths-services)
- [Scheduling Services](#scheduling-services)
- [Slug Services](#slug-services)
- [Template Services](#template-services)
- [Title Services](#title-services)

---

## Core Services

### AzureWebAppPublisher

**Namespace:** `CosmosCMS.Editor.Services`

A service for publishing files to the wwwroot directory of an Azure Web App using the Kudu API.

**Key Methods:**
- `PublishFileToWwwrootAsync(string filePath)` - Publishes a file to the wwwroot directory using the Kudu API
- `DeleteFileAtWwwrootAsync(string filePath)` - Deletes a file from the wwwroot directory using the Kudu API

**Constructor Parameters:**
- `webAppName` - Azure Web App name
- `resourceGroupName` - Azure resource group name
- `subscriptionId` - Azure subscription ID
- `credential` - Azure default credential for authentication

### CosmosDbService

**Namespace:** `Sky.Editor.Data`

A service class to interact with Cosmos DB for querying and data retrieval.

**Key Methods:**
- `QueryWithGroupByAsync(string sqlQuery)` - Queries the Cosmos DB container and returns a dynamic list of results
- `QueryWithGroupByAsync<T>(string sqlQuery)` - Generic version that returns a strongly-typed list of results

**Constructor Parameters:**
- `cosmosClient` - Cosmos DB Client
- `databaseName` - Database name
- `containerName` - Container name

### HangfireDashboardAuthorizationFilter

**Namespace:** `Sky.Editor.Services`

Authorization filter for Hangfire Dashboard that restricts access to authenticated administrators.

**Key Methods:**
- `Authorize(DashboardContext context)` - Returns true if user is authenticated and in the "Administrators" role

**Implements:** `IDashboardAuthorizationFilter`

### HtmlUtilities

**Namespace:** `Sky.Cms.Services`

HTML utility class for URL manipulation and HTML processing.

**Key Methods:**
- `IsAbsoluteUri(string url)` - Determines if a URL is an absolute URI
- `RelativeToAbsoluteUrls(string html, Uri absoluteUrl, bool isLayoutBodyElement)` - Converts relative URIs to absolute URIs in HTML content

**Use Cases:**
- Converting relative links to absolute links for publishing
- Processing HTML content for layout elements

### ImageResizer

**Namespace:** `Sky.Cms.Services`

Image resize utility for calculating new image dimensions while maintaining aspect ratios.

**Key Methods:**
- `Resize(ImageSizeModel originalSize, ImageSizeModel targetSize)` - Calculates resized dimensions based on original and target sizes

**Returns:** `ImageSizeModel` with calculated width and height

### IStartupTaskService

**Namespace:** `Cosmos.Editor.Services`

Service interface for running startup tasks asynchronously.

**Key Methods:**
- `RunAsync()` - Runs the startup tasks asynchronously

### MultiDatabaseManagementUtilities

**Namespace:** `Cosmos.Editor.Services`

Utility class for querying and updating multiple Cosmos DB databases based on configured connections. Supports multi-tenant configurations.

**Key Properties:**
- `IsMultiTenant` - Indicates whether the application is configured for multi-tenancy

**Key Methods:**
- `GetDomainsByEmailAddress(string emailAddress)` - Retrieves domains associated with an email address across all databases
- `UpdateIdentityUser(IdentityUser identityUser)` - Updates user information across all databases where the user exists
- `GetConnections()` - Retrieves all connections from the dynamic configuration database
- `EnsureDatabasesAreConfigured()` - Ensures all databases are configured and created

**Use Cases:**
- Multi-tenant database management
- Cross-database user synchronization
- Database configuration validation

### SetupNewAdministrator

**Namespace:** `Cosmos.Editor.Services`

Static class for creating and managing administrator roles and initial administrator setup.

**Key Methods:**
- `Ensure_RolesAndAdmin_Exists(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, IdentityUser user)` - Ensures required roles exist and adds the first user as administrator
- `Ensure_Roles_Exists(RoleManager<IdentityRole> roleManager)` - Ensures all required identity roles exist

**Use Cases:**
- Initial application setup
- First-time administrator creation
- Role configuration

### StartupTaskService

**Namespace:** `Cosmos.Editor.Services`

Service implementation for running startup tasks asynchronously, including file uploads to Azure Blob Storage.

**Key Methods:**
- `RunAsync()` - Runs startup tasks including uploading required files to all configured storage connections

**Constructor Parameters:**
- `webHost` - Web host environment
- `managementUtilities` - Database management utilities

**Implements:** `IStartupTaskService`

### ThumbnailCreator

**Namespace:** `Sky.Cms.Services`

Image thumbnail creator for generating thumbnail images from source images.

**Key Methods:**
- `Create(Stream source, ImageSizeModel desiredSize, string contentType)` - Creates a thumbnail maintaining aspect ratio
- `CreateFill(Stream source, ImageSizeModel desiredSize, string contentType)` - Creates a thumbnail filling the entire space

**Supported Image Formats:**
- PNG (image/png)
- GIF (image/gif)
- JPEG (image/jpeg)

### ViewRenderingService

**Namespace:** `Sky.Cms.Services`

Service for rendering Razor views to strings, useful for generating HTML content programmatically.

**Key Methods:**
- `RenderToStringAsync(string viewName, object model)` - Renders a view to a string with the provided model

**Implements:** `IViewRenderService`

**Credits:** Based on Stack Overflow community contributions

---

## Author Services

### IAuthorInfoService

**Location:** `Services\Authors\IAuthorInfoService.cs`

Interface for author information services.

### AuthorInfoService

**Location:** `Services\Authors\AuthorInfoService.cs`

Implementation of author information services.

---

## Blog Publishing Services

### IBlogRenderingService

**Location:** `Services\BlogPublishing\IBlogRenderingService.cs`

Interface for blog rendering services.

### BlogRenderingService

**Location:** `Services\BlogPublishing\BlogRenderingService.cs`

Implementation of blog rendering and publishing services.

---

## Catalog Services

### ICatalogService

**Location:** `Services\Catalog\ICatalogService.cs`

Interface for catalog services.

### CatalogService

**Location:** `Services\Catalog\CatalogService.cs`

Implementation of catalog management services.

---

## CDN Services

### ICdnDriver

**Location:** `Services\CDN\ICdnDriver.cs`

Interface for CDN (Content Delivery Network) driver implementations.

### CdnService

**Location:** `Services\CDN\CdnService.cs`

Main CDN service for managing content delivery.

### AzureCdnDriver

**Location:** `Services\CDN\AzureCdnDriver.cs`

CDN driver implementation for Azure CDN.

**Related Classes:**
- `AzureCdnConfig` - Configuration settings for Azure CDN

### CloudflareCdnDriver

**Location:** `Services\CDN\CloudflareCdnDriver.cs`

CDN driver implementation for Cloudflare CDN.

**Related Classes:**
- `CloudflareCdnConfig` - Configuration settings for Cloudflare CDN

### SucuriCdnService

**Location:** `Services\CDN\SucuriCdnService.cs`

CDN service implementation for Sucuri CDN.

**Related Classes:**
- `SucuriCdnConfig` - Configuration settings for Sucuri CDN

### Supporting CDN Classes

- **CdnProviderEnum** - Enumeration of supported CDN providers
- **CdnResult** - Result object for CDN operations
- **CdnSetting** - CDN configuration settings
- **AllOrNoneRequiredAttribute** - Validation attribute for CDN configuration

---

## HTML Services

### IArticleHtmlService

**Location:** `Services\Html\IArticleHtmlService.cs`

Interface for article HTML processing services.

### ArticleHtmlService

**Location:** `Services\Html\ArticleHtmlService.cs`

Implementation of article HTML processing and manipulation services.

---

## Publishing Services

### IPublishingService

**Location:** `Services\Publishing\IPublishingService.cs`

Interface for content publishing services.

### PublishingService

**Location:** `Services\Publishing\PublishingService.cs`

Implementation of content publishing services for deploying content to various targets.

---

## Redirect Services

### IRedirectService

**Location:** `Services\Redirects\IRedirectService.cs`

Interface for URL redirect services.

### RedirectService

**Location:** `Services\Redirects\RedirectService.cs`

Implementation of URL redirect management services.

---

## Reserved Paths Services

### IReservedPaths

**Location:** `Services\ReservedPaths\IReservedPaths.cs`

Interface for managing reserved URL paths.

### ReservedPaths

**Location:** `Services\ReservedPaths\ReservedPaths.cs`

Implementation of reserved URL path management to prevent conflicts with system routes.

---

## Scheduling Services

### IArticleScheduler

**Location:** `Services\Scheduling\IArticleScheduler.cs`

Interface for article scheduling services.

**Key Methods:**

- `ExecuteAsync()` - Executes the scheduled job to process article versions with multiple published dates

### ArticleScheduler

**Location:** `Services\Scheduling\ArticleScheduler.cs`

Implementation of article scheduling for timed content publication using Hangfire. Automatically publishes web pages that content creators have scheduled for future dates.

**Key Features:**

- Runs every 10 minutes via Hangfire recurring job
- Supports both single-tenant and multi-tenant modes
- Manages article versions with scheduled publication dates
- Automatically activates the most recent non-future version
- Unpublishes older versions when new versions go live

**Key Methods:**

- `ExecuteAsync()` - Main entry point for the Hangfire recurring job
- `Run(ApplicationDbContext, string)` - Processes articles for a single tenant/database
- `ProcessArticleVersions(DateTimeOffset, ApplicationDbContext, int)` - Handles version activation logic for a specific article

**Constructor Parameters:**

- `dbContext` - Database context for querying articles
- `config` - Cosmos configuration options
- `memoryCache` - Memory cache for performance optimization
- `storageContext` - Storage context for file operations
- `logger` - Logger for tracking execution and errors
- `accessor` - HTTP context accessor
- `settings` - Editor settings
- `clock` - Clock abstraction for testable time operations
- `slugService` - URL slug generation service
- `htmlService` - HTML processing service
- `catalogService` - Catalog management service
- `publishingService` - Publishing service for generating static files
- `titleChangeService` - Service for handling title changes
- `redirectService` - Redirect management service
- `templateService` - Template service
- `configurationProvider` - Optional configuration provider for multi-tenant mode

**Related Documentation:**

- [Page Scheduling Guide](../../Docs/Editors/PageScheduling.md) - Complete user and developer documentation
- [Hangfire Dashboard](https://docs.hangfire.io/) - Background job monitoring

### HangfireAuthorizationFilter

**Location:** `Services\Scheduling\HangfireAuthorizationFilter.cs`

Authorization filter for restricting access to the Hangfire dashboard.

**Key Methods:**

- `Authorize(DashboardContext)` - Determines if the current user can access the dashboard

**Authorization Rules:**

- User must be authenticated
- User must be in Administrator or Editor role

### HangFireExtensions

**Location:** `Services\Scheduling\HangFireExtensions.cs`

Extension methods for configuring Hangfire with SkyCMS.

**Key Methods:**

- `AddHangFireScheduling(IServiceCollection, IConfiguration)` - Configures Hangfire with appropriate database storage

**Supported Databases:**

- Cosmos DB
- SQL Server
- MySQL
- SQLite (for testing)

---

## Slug Services

### ISlugService

**Location:** `Services\Slugs\ISlugService.cs`

Interface for URL slug generation and management services.

### SlugService

**Location:** `Services\Slugs\SlugService.cs`

Implementation of URL slug generation and management for SEO-friendly URLs.

---

## Template Services

### ITemplateService

**Location:** `Services\Templates\ITemplateService.cs`

Interface for template management services.

### TemplateService

**Location:** `Services\Templates\TemplateService.cs`

Implementation of template management services for page layouts and designs.

### PageTemplate

**Location:** `Services\Templates\PageTemplate.cs`

Model class representing a page template.

---

## Title Services

### ITitleChangeService

**Location:** `Services\Titles\ITitleChangeService.cs`

Interface for managing title changes and related operations.

### TitleChangeService

**Location:** `Services\Titles\TitleChangeService.cs`

Implementation of title change management services.

---

## Architecture Notes

### Dependency Injection

Most services in this folder are designed to be registered with the ASP.NET Core dependency injection container. Services typically have interfaces (prefixed with `I`) that define their contracts.

### Azure Integration

Several services integrate with Azure services:

- **AzureWebAppPublisher** - Azure App Service integration
- **AzureCdnDriver** - Azure CDN integration
- **CosmosDbService** - Azure Cosmos DB integration
- **MultiDatabaseManagementUtilities** - Multi-database Cosmos DB management

### Multi-Tenancy Support

The `MultiDatabaseManagementUtilities` class provides comprehensive support for multi-tenant configurations, allowing the system to manage multiple databases and synchronize data across tenants.

### Background Processing

Services like `ArticleScheduler` and `StartupTaskService` support background task execution using Hangfire for scheduling and asynchronous operations.

---

## SkyCMS vs Traditional CMS Systems

### Traditional CMS Characteristics

When comparing SkyCMS to traditional content management systems like WordPress, Drupal, Joomla, Umbraco, Sitecore, and Adobe Experience Manager, it's important to understand the traditional approach:

**Traditional CMS Architecture:**

- **Monolithic Design** - All components (content management, presentation, database) tightly coupled in a single system
- **Relational Databases** - Typically MySQL, PostgreSQL, or SQL Server with fixed schemas
- **Server-Side Rendering** - Pages generated on-demand for each request with limited caching
- **WYSIWYG Editors** - Visual editing interfaces showing approximate final output
- **Theme/Template Systems** - Pre-built templates controlling both structure and presentation
- **Plugin/Extension Ecosystems** - Modular functionality through third-party plugins

**Common Pain Points:**

- Performance bottlenecks from server-side rendering
- Scaling challenges with monolithic architecture
- Security vulnerabilities from large plugin ecosystems
- Tight coupling between content and presentation layers
- Limited API capabilities for multi-channel content delivery
- Complex upgrade paths and maintenance overhead

### SkyCMS Modern Cloud-Native Approach

SkyCMS takes a fundamentally different approach, leveraging modern cloud technologies and architectural patterns:

#### 1. **Cloud-Native Architecture**

- **Azure Cosmos DB** - Globally distributed, multi-model NoSQL database providing:
  - Automatic horizontal scaling
  - Multi-region replication
  - 99.999% availability SLA
  - Flexible schema design
  - Low-latency data access worldwide

- **Azure Integration** - Native cloud services throughout:
  - Azure Blob Storage for static assets
  - Azure CDN for content delivery
  - Azure App Service for hosting
  - Azure Identity for authentication

#### 2. **Multi-Tenancy by Design**

Unlike traditional CMS systems that require complex workarounds for multi-site management, SkyCMS is built from the ground up for multi-tenancy:

- `MultiDatabaseManagementUtilities` manages multiple databases seamlessly
- Cross-tenant user synchronization
- Isolated data per tenant with shared infrastructure
- Centralized configuration management through `DynamicConfigDbContext`

#### 3. **Decoupled Content Delivery**

- **Multiple CDN Support** - Azure CDN, Cloudflare, and Sucuri through pluggable drivers
- **Static Site Generation** - Content can be published to static sites for maximum performance
- **Edge Caching** - Content delivered from edge locations closest to users
- **Publishing Service** - Flexible deployment to various targets

#### 4. **Superior Performance & Scalability**

- **Background Processing** - Hangfire for scheduled tasks and async operations
- **Image Optimization** - Built-in thumbnail generation and image resizing
- **View Caching** - `ViewRenderingService` for pre-rendered content
- **Distributed Architecture** - Scales horizontally without bottlenecks

#### 5. **Developer-Friendly**

- **ASP.NET Core** - Modern, high-performance web framework
- **Dependency Injection** - Clean architecture with testable components
- **Interface-Based Design** - Easy to extend and customize
- **Strong Typing** - C# type safety reduces runtime errors

#### 6. **Modern Content Management**

- **Blog Publishing** - Dedicated `BlogRenderingService` for content creation
- **SEO Optimization** - `SlugService` for SEO-friendly URLs
- **Scheduling** - `ArticleScheduler` for timed content publication
- **HTML Processing** - Advanced HTML utilities for content transformation

#### 7. **Enterprise Features**

- **Security** - Role-based access control with `HangfireDashboardAuthorizationFilter`
- **URL Management** - `RedirectService` and `ReservedPaths` for proper URL handling
- **Template Management** - Flexible `TemplateService` for page layouts
- **Catalog System** - Built-in catalog management capabilities

### Key Benefits Over Traditional CMS

| Feature | Traditional CMS | SkyCMS |
|---------|----------------|---------|
| **Scalability** | Vertical scaling, limited | Horizontal, globally distributed |
| **Performance** | Server-side rendering per request | Edge caching, static generation |
| **Multi-Site** | Complex plugins, single database | Native multi-tenancy support |
| **Availability** | Single region, 95-99% | Multi-region, 99.999% SLA |
| **Maintenance** | Plugin updates, security patches | Managed cloud services |
| **API-First** | Retrofitted APIs | Built with modern API patterns |
| **Cost Model** | Fixed infrastructure costs | Pay-per-use, scales with demand |
| **Global Reach** | Requires CDN plugins | CDN integration built-in |
| **Development** | PHP, legacy patterns | Modern C#, .NET Core |
| **Content Delivery** | Monolithic rendering | Decoupled, multi-channel ready |

### Use Cases Where SkyCMS Excels

1. **Global Websites** - Multi-region content delivery with low latency worldwide
2. **Multi-Tenant SaaS** - Managing hundreds or thousands of sites from a single platform
3. **High-Traffic Sites** - Scales automatically to handle traffic spikes
4. **Enterprise Publishing** - Complex workflows, scheduling, and content management
5. **Headless CMS** - API-first architecture for modern frontend frameworks
6. **E-commerce Catalogs** - Built-in catalog service for product management
7. **Blog Platforms** - Dedicated blog publishing services

---

*Last Updated: November 11, 2025*
