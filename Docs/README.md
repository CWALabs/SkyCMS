# SkyCMS Documentation

**Welcome to the SkyCMS documentation hub.** This page provides organized access to all SkyCMS documentation, guides, and resources.

For a project overview and introduction to SkyCMS, see the [main README](../README.md).

---

## Table of Contents

- [Getting Started](#-getting-started)
- [Installation & Deployment](#-installation--deployment)
- [Configuration](#-configuration)
- [Content Management](#-content-management)
- [Blogging](#-blogging)
- [Editing Tools](#-editing-tools)
- [For Developers](#-for-developers)
- [Architecture & Components](#-architecture--components)
- [Comparisons & Use Cases](#-comparisons--use-cases)
- [Additional Resources](#-additional-resources)

---

## Getting Started

Start here if you're new to SkyCMS.

- **[Quick Start Guide](./QuickStart.md)** - Get up and running quickly
- **[Developer Experience](./DeveloperExperience.md)** - Overview for developers
- **[Cost Comparison](./CostComparison.md)** - Understanding SkyCMS costs vs alternatives

---

## Installation & Deployment

### Cloud Platform Guides

- **[Azure Installation](./AzureInstall.md)** - Deploy SkyCMS to Microsoft Azure
- **[AWS Deployment](./S3StaticWebsite.md)** - Deploy using S3 static hosting (CloudFormation/ECS guide coming soon)
- **[Azure ARM Templates](../ArmTemplates/README.md)** - One-click Azure deployment templates

### Specialized Hosting

- **[Cloudflare Edge Hosting](./CloudflareEdgeHosting.md)** - Origin-less hosting with Cloudflare R2 + Rules
- **[S3 Static Website Hosting](./S3StaticWebsite.md)** - Host as a static site on AWS S3

### Access Keys & Configuration

- **[AWS S3 Access Keys](./AWS-S3-AccessKeys.md)** - Quick guide to obtaining S3 credentials
- **[Cloudflare R2 Access Keys](./Cloudflare-R2-AccessKeys.md)** - Quick guide to obtaining R2 credentials

---

## Configuration

Essential configuration guides for databases and storage.

- **[Database Configuration](./DatabaseConfig.md)** - Provider options (Cosmos DB, SQL Server, MySQL), connection strings, EF configuration, and migrations
- **[Storage Configuration](./StorageConfig.md)** - Cloud storage providers (Azure Blob, AWS S3, Cloudflare R2), container/bucket naming, CDN integration, and recommendations

---

## Content Management

### Page & Layout Management

- **[Layouts Guide](./Layouts/Readme.md)** - Creating and managing site-wide layouts (headers, footers, site structure)
- **[Page Templates Guide](./Templates/Readme.md)** - Reusable page structures and managing template-based pages
- **[Page Scheduling](./Editors/PageScheduling.md)** - Schedule pages for automatic future publication
- **[Migration: Save Article Pipeline](./MIGRATION-SAVE-ARTICLE.md)** - Internals of article persistence and versioning

### File Management

- **[File Management Guide](./FileManagement/README.md)** - Managing media files and assets
  - [Quick Start](./FileManagement/Quick-Start.md)
  - [Code Editing Files](./FileManagement/Code-Editing.md)
  - [Image Editing](./FileManagement/Image-Editing.md)

---

## 📰 Blogging

Comprehensive guides for SkyCMS blogging features.

- **[Blog Post Lifecycle](./blog/BlogPostLifecycle.md)** - Creating, editing, and publishing blog posts
- **[Future Blog Enhancements](./blog/BlogFutureEnhancements.md)** - Upcoming blog functionality and roadmap

---

## ✏️ Editing Tools

SkyCMS integrates multiple powerful editors for different workflows.

### Content Editors

- **[Live Editor (CKEditor 5)](./Editors/LiveEditor/README.md)** - WYSIWYG editing for non-technical users
  - [Quick Start](./Editors/LiveEditor/QuickStart.md)
  - [Visual Guide](./Editors/LiveEditor/VisualGuide.md)
  - [Technical Reference](./Editors/LiveEditor/TechnicalReference.md)
- **[Designer (GrapesJS)](./Editors/Designer/README.md)** - Visual drag-and-drop page builder
  - [Quick Start](./Editors/Designer/QuickStart.md)
- **[Code Editor (Monaco)](./Editors/CodeEditor/README.md)** - HTML/CSS/JavaScript editing for developers
- **[Image Editing (Filerobot)](./Editors/ImageEditing/README.md)** - Integrated image editing capabilities

---

## 👨‍💻 For Developers

Technical documentation for customization and development.

### Controllers & Base Classes

- **[Developer Overview](./Developers/README.md)** - Developer documentation hub
- **[HomeControllerBase](./Developers/Controllers/HomeControllerBase.md)** - Base controller for content pages
- **[PubControllerBase](./Developers/Controllers/PubControllerBase.md)** - Base controller for public-facing pages

### Widgets & Components

- **[Widgets Overview](./Widgets/README.md)** - Reusable UI components and widgets
  - [Image Widget](./Widgets/Image-Widget.md) - Image upload and management
  - [Crypto Widget](./Widgets/Crypto-Widget.md) - AES encryption/decryption helpers
  - [Crumbs Widget](./Widgets/Crumbs-Widget.md) - Breadcrumb navigation
  - [Forms Widget](./Widgets/Forms-Widget.md) - Form handling and antiforgery tokens
  - [Nav Builder Widget](./Widgets/Nav-Builder-Widget.md) - Navigation list rendering
  - [Search Widget](./Widgets/Search-Widget.md) - Search form and results
  - [ToC Widget](./Widgets/ToC-Widget.md) - Table of contents generation

### Additional Developer Resources

- **[Image Widget Development](./Developers/ImageWidget.md)** - Deep dive into image widget implementation

---

## Architecture & Components

Learn about SkyCMS's internal structure and libraries.

### Core Components

- **[Editor Application](../Editor/README.md)** - Content management interface
- **[Publisher Application](../Publisher/README.md)** - Public-facing website renderer
- **[Common Library](../Common/README.md)** - Shared functionality and utilities
- **[Blob Service](../Cosmos.BlobService/README.md)** - Multi-cloud file storage service
- **[Dynamic Configuration](../Cosmos.ConnectionStrings/README.md)** - Runtime configuration management
- **[Identity Framework](../AspNetCore.Identity.FlexDb/README.md)** - Flexible authentication system

---

## 🆚 Comparisons & Use Cases

Understand how SkyCMS compares to other solutions.

- **[SkyCMS vs Headless CMS](./CosmosVsHeadless.md)** - Understanding the edge-native CMS approach
- **[SkyCMS Competitors Analysis](./SkyCMS-Competitors.md)** - How SkyCMS compares to alternatives
- **[Migrating from JAMstack](./MigratingFromJAMstack.md)** - Moving from Git-based static site workflows
- **[Cost Comparison](./CostComparison.md)** - Operational cost analysis

---

## Additional Resources

### Marketing & Content

- **[SkyCMS Homepage Content (HTML)](./SkyCMS-Homepage-Content.html)** - Marketing page HTML
- **[SkyCMS Homepage Content (Markdown)](./SkyCMS-Homepage-Content.md)** - Marketing page markdown
- **[Azure Marketplace Description](./AzureMarketplaceDescription.html)** - Azure Marketplace listing content

### License & Legal

- **[License Information](./License.md)** - Dual licensing (GPL 2.0-or-later / MIT) details
- **[Third-Party Notices](../NOTICE.md)** - Attribution and licenses for included components

### Project Links

- **[GitHub Repository](https://github.com/MoonriseSoftwareCalifornia/SkyCMS)** - Source code and issue tracking
- **[Docker Hub - Editor](https://hub.docker.com/r/toiyabe/sky-editor)** - Editor container images
- **[Docker Hub - Publisher](https://hub.docker.com/r/toiyabe/sky-publisher)** - Publisher container images
- **[Docker Hub - API](https://hub.docker.com/r/toiyabe/sky-api)** - API container images
- **[Alternative NodeJS Publisher](https://github.com/MoonriseSoftwareCalifornia/Sky.Publisher.NodeJs)** - NodeJS-based Publisher implementation

---

## 🆘 Getting Help

### Documentation Not Found?

If you're looking for documentation that doesn't exist yet or needs updating:

1. **Check the Component READMEs** - Each major component has its own README with detailed documentation
2. **Search the Codebase** - Many features have inline documentation and comments
3. **GitHub Issues** - Report missing documentation or request new guides
4. **Community** - Engage with the SkyCMS community for help

### Support Channels

- **GitHub Issues** - [Report bugs and request features](https://github.com/MoonriseSoftwareCalifornia/SkyCMS/issues)
- **Discussions** - Community support and questions
- **Documentation Contributions** - Pull requests welcome for documentation improvements

---

## Documentation Conventions

Throughout this documentation:

- **Bold** - Important terms and UI elements
- `Code blocks` - File names, commands, and code snippets
- **Icons** - Visual organization and quick reference
- [Links] - Cross-references to related documentation

---

**Last Updated:** November 2025  
**Maintained by:** Moonrise Software, LLC  
**License:** See [LICENSE-GPL](../LICENSE-GPL) and [LICENSE-MIT](../LICENSE-MIT)

For project overview and "what is SkyCMS," see the [main README](../README.md).
