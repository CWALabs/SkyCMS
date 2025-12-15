
# SkyCMS Documentation

**Welcome to the SkyCMS documentation hub.** This page provides organized access to all SkyCMS documentation, guides, and resources.

For a project overview and introduction to SkyCMS, see the [main README](../README.md).

**Documentation Version:** 2.0 (December 2025)  
**Compatible with:** SkyCMS v2.x  
**Last Updated:** December 15, 2025

> **New to SkyCMS?** Check out our [**Learning Paths Guide**](./LEARNING_PATHS.md) for role-based documentation journeys!

---

## Table of Contents

- [Learning Paths](#learning-paths)
- [Getting Started](#getting-started)
- [Installation & Deployment](#installation--deployment)
- [Configuration](#configuration)
- [Authentication & Authorization](#authentication--authorization)
- [Content Management](#content-management)
- [Publishing](#publishing)
- [Blogging](#blogging)
- [Editing Tools](#editing-tools)
- [Widgets](#widgets)
- [For Developers](#for-developers)
- [Architecture & Components](#architecture--components)
- [Comparisons & Use Cases](#comparisons--use-cases)
- [Troubleshooting](#troubleshooting)
- [Additional Resources](#additional-resources)

---

## Learning Paths

**Choose your role and follow a curated documentation journey:**

- [Content Editor (Non-Technical)](./LEARNING_PATHS.md#-content-editor-non-technical) - 30-45 minutes
- [Developer](./LEARNING_PATHS.md#-developer) - 2-3 hours
- [DevOps / System Administrator](./LEARNING_PATHS.md#️-devops--system-administrator) - 3-4 hours
- [Decision Maker / Manager](./LEARNING_PATHS.md#-decision-maker--manager) - 30-45 minutes

See the complete [Learning Paths Guide](./LEARNING_PATHS.md) for detailed step-by-step instructions.

---

## Getting Started

Start here if you're new to SkyCMS.

- **[About SkyCMS](./About.md)** - What SkyCMS is and who it's for
- **[Quick Start Guide](./QuickStart.md)** - Get up and running quickly
- **[Developer Experience](./DeveloperExperience.md)** - Overview for developers
- **[Migrating from JAMstack](./MigratingFromJAMstack.md)** - Moving from Git-based static site workflows

---

## Installation & Deployment

### Cloud Platform Guides

- **[Azure Installation](./Installation/AzureInstall.md)** - Deploy SkyCMS to Microsoft Azure
- **[AWS S3 Static Website Hosting](./S3StaticWebsite.md)** - Deploy using S3 static hosting
- **[Cloudflare Edge Hosting](./Installation/CloudflareEdgeHosting.md)** - Origin-less hosting with Cloudflare R2 + Rules

---

## Configuration

Essential configuration guides for databases, storage, and CDN.

- **[Configuration Overview](./Configuration/README.md)** - Quick reference for all configuration documentation
- **[Database Configuration](./Configuration/Database-Overview.md)** - Supported providers, connection string formats, and setup steps
  - [Azure Cosmos DB](./Configuration/Database-CosmosDB.md)
  - [MS SQL Server / Azure SQL](./Configuration/Database-SQLServer.md)
  - [MySQL](./Configuration/Database-MySQL.md)
  - [SQLite](./Configuration/Database-SQLite.md)
- **[Storage Configuration](./Configuration/Storage-Overview.md)** - Supported providers, connection string formats, and setup steps
  - [Azure Blob Storage](./Configuration/Storage-AzureBlob.md)
  - [Amazon S3](./Configuration/Storage-S3.md)
  - [Cloudflare R2](./Configuration/Storage-Cloudflare.md)
  - [Google Cloud Storage](./Configuration/Storage-GoogleCloud.md)
- **[CDN Integration](./Configuration/CDN-Overview.md)** - Supported providers, required values, and where to configure in SkyCMS
  - [Azure Front Door CDN](./Configuration/CDN-AzureFrontDoor.md)
  - [Cloudflare CDN](./Configuration/CDN-Cloudflare.md)
  - [Amazon CloudFront CDN](./Configuration/CDN-CloudFront.md)
  - [Sucuri CDN/WAF](./Configuration/CDN-Sucuri.md)

---

## Authentication & Authorization

Secure access and permission management for editors and users.

- **[Authentication Overview](./Authentication-Overview.md)** - Authentication methods, concepts, and setup guide
  - Local username/password authentication
  - Azure Active Directory (Azure AD)
  - Azure B2C for consumer identity
  - OpenID Connect / OAuth 2.0 for custom providers
- **[Identity Framework](./Components/AspNetCore.Identity.FlexDb.md)** - Flexible identity system for multiple databases

---

## Content Management

### Page & Layout Management

- **[Layouts Guide](./Layouts/Readme.md)** - Creating and managing site-wide layouts (headers, footers, site structure)
- **[Page Templates Guide](./Templates/Readme.md)** - Reusable page structures and managing template-based pages
- **[Page Scheduling](./Editors/PageScheduling.md)** - Schedule pages for automatic future publication
- **[Migration: Save Article Pipeline](./MIGRATION-SAVE-ARTICLE.md)** - Internals of article persistence and versioning

### File & Media Management

- **[File Management Overview](./FileManagement/index.md)** - Entry point for file and media documentation
- **[Complete Guide](./FileManagement/README.md)** - Comprehensive file management reference
- **[Quick Start](./FileManagement/Quick-Start.md)** - Get started in 5-10 minutes

---

## Publishing

Publishing workflows, modes, and best practices.

- **[Publishing Overview](./Publishing-Overview.md)** - Publishing modes, workflows, and strategies
  - Direct publishing for rapid updates
  - Staged publishing for review workflows
  - Static generation for JAMstack deployment
  - Git-based publishing for CI/CD pipelines
- **[Scheduled Publishing](./Editors/PageScheduling.md)** - Schedule content for automatic publication

---

## Blogging

Comprehensive guides for SkyCMS blogging features.

- **[Blog Post Lifecycle](./blog/BlogPostLifecycle.md)** - Creating, editing, and publishing blog posts
- **[Future Blog Enhancements](./blog/BlogFutureEnhancements.md)** - Upcoming blog functionality and roadmap

---

## Editing Tools

SkyCMS integrates multiple powerful editors for different workflows.

### Live Editor (CKEditor 5)

WYSIWYG editing for content creators with inline editing capabilities.

- **[Complete Guide](./Editors/LiveEditor/README.md)** - Full feature documentation and reference
- **[Quick Start](./Editors/LiveEditor/QuickStart.md)** - Get started in 5 minutes
- **[Visual Guide](./Editors/LiveEditor/VisualGuide.md)** - Interface diagrams and visual reference
- **[Technical Reference](./Editors/LiveEditor/TechnicalReference.md)** - Developer documentation

### Designer (GrapesJS)

Visual drag-and-drop page builder for creating layouts without code.

- **[Designer Guide](./Editors/Designer/README.md)** - Complete documentation
- **[Quick Start](./Editors/Designer/QuickStart.md)** - Get started quickly

### Code Editor (Monaco)

Professional code editor for HTML, CSS, and JavaScript with syntax highlighting and IntelliSense.

- **[Code Editor Guide](./Editors/CodeEditor/README.md)** - Complete documentation

### Image Editing (Filerobot)

Integrated browser-based image editor for cropping, resizing, and adjusting images.

- **[Image Editing Guide](./Editors/ImageEditing/README.md)** - Complete documentation

---

## Widgets

Reusable UI components for content and developers.

- **[Widgets Overview](./Widgets-Overview.md)** - Understanding widgets and when to use them
- **[Widgets Directory](./Widgets/README.md)** - Complete widget documentation and API reference
  - [Image Widget](./Widgets/Image-Widget.md) - Image upload and management
  - [Crypto Widget](./Widgets/Crypto-Widget.md) - Encryption/decryption helpers
  - [Crumbs Widget](./Widgets/Crumbs-Widget.md) - Breadcrumb navigation
  - [Forms Widget](./Widgets/Forms-Widget.md) - Form handling and submissions
  - [Nav Builder Widget](./Widgets/Nav-Builder-Widget.md) - Navigation menu generation
  - [Search Widget](./Widgets/Search-Widget.md) - Full-text search functionality
  - [ToC Widget](./Widgets/ToC-Widget.md) - Table of contents generation

---

## For Developers

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

### Core Applications

- **[Editor Application](../Editor/README.md)** - Content management interface
- **[Publisher Application](../Publisher/README.md)** - Public-facing website renderer

### Component Libraries

- **[Common Library](./Components/Cosmos.Common.md)** - Core CMS library ([full docs](../Common/README.md))
- **[Blob Service](./Components/Cosmos.BlobService.md)** - Multi-cloud storage ([full docs](../Cosmos.BlobService/README.md))
- **[Identity Framework](./Components/AspNetCore.Identity.FlexDb.md)** - Flexible database identity ([full docs](../AspNetCore.Identity.FlexDb/README.md))
- **[Dynamic Configuration](../Cosmos.ConnectionStrings/README.md)** - Runtime configuration management

### Development & Testing

- **[Testing Guide](./Development/Testing/README.md)** - Comprehensive testing documentation

---

## Comparisons & Use Cases

Understand how SkyCMS compares to other solutions.

- **[SkyCMS vs Headless CMS](./CosmosVsHeadless.md)** - Understanding the edge-native CMS approach
- **[Migrating from JAMstack](./MigratingFromJAMstack.md)** - Moving from Git-based static site workflows

### Marketing & Competitive Materials

For detailed competitive analysis and cost comparisons, see the [Marketing Materials folder](./_Marketing/README.md):
- [Competitor Analysis](./_Marketing/Competitor-Analysis.md) - Detailed comparison with alternatives
- [Cost Comparison](./_Marketing/Cost-Comparison.md) - Operational cost analysis

---

## Troubleshooting

- **[Troubleshooting Guide](./Troubleshooting.md)** - Solutions for common setup and operational issues organized by feature area (database, storage, CDN, publishing, authentication, performance)

---

## Additional Resources

### Release Information

- **[Changelog](./CHANGELOG.md)** - Version history and release notes

### Marketing & Sales Materials

See the [Marketing Materials folder](./_Marketing/README.md) for:
- Website content and homepage copy
- Azure Marketplace listing content
- Competitive analysis and positioning
- Cost comparisons with alternatives

### License & Legal

- **[License Information](./License.md)** - Dual licensing (GPL 2.0-or-later / MIT) details
- **[Third-Party Notices](../NOTICE.md)** - Attribution and licenses for included components
- **[GPL License](../LICENSE-GPL)** - Full GPL 2.0-or-later license text
- **[MIT License](../LICENSE-MIT)** - Full MIT license text

### Project Links

- **[GitHub Repository](https://github.com/CWALabs/SkyCMS)** - Source code and issue tracking
- **[Docker Hub - Editor](https://hub.docker.com/r/toiyabe/sky-editor)** - Editor container images
- **[Docker Hub - Publisher](https://hub.docker.com/r/toiyabe/sky-publisher)** - Publisher container images
- API container image - coming soon
- Alternative NodeJS Publisher - coming soon

---

## Getting Help

### Documentation Not Found?

If you're looking for documentation that doesn't exist yet or needs updating:

1. **Check the Component READMEs** - Each major component has its own README with detailed documentation
2. **Search the Codebase** - Many features have inline documentation and comments
3. **GitHub Issues** - Report missing documentation or request new guides
4. **Community** - Engage with the SkyCMS community for help

### Support Channels

- **GitHub Issues** - [Report bugs and request features](https://github.com/CWALabs/SkyCMS/issues)
- **Discussions** - Community support and questions (via GitHub Issues if Discussions are unavailable)
- **Documentation Contributions** - Pull requests welcome for documentation improvements

---

## Documentation Conventions

Throughout this documentation:

- **Bold** - Important terms and UI elements
- `Code blocks` - File names, commands, and code snippets
- [Links] - Cross-references to related documentation

---

**Maintained by:** Moonrise Software, LLC  
**License:** Dual-licensed under GPL 2.0-or-later and MIT

For project overview and "what is SkyCMS," see the [main README](../README.md).

---

## Complete Documentation Index

For an exhaustive list of all documentation files, see below:

<details>
<summary><b>Click to expand complete file listing</b></summary>

### Core Documentation
- Getting Started: [About SkyCMS](./About.md) | [Quick Start](./QuickStart.md) | [LEARNING_PATHS](./LEARNING_PATHS.md)
- Overview Documents: [Authentication](./Authentication-Overview.md) | [Publishing](./Publishing-Overview.md) | [Widgets](./Widgets-Overview.md)

### Configuration Guides
- Database: [Overview](./Configuration/Database-Overview.md) | [Reference](./Configuration/Database-Configuration-Reference.md)
  - [Azure Cosmos DB](./Configuration/Database-CosmosDB.md) | [MS SQL Server](./Configuration/Database-SQLServer.md) | [MySQL](./Configuration/Database-MySQL.md) | [SQLite](./Configuration/Database-SQLite.md)
- Storage: [Overview](./Configuration/Storage-Overview.md) | [Reference](./Configuration/Storage-Configuration-Reference.md)
  - [Azure Blob](./Configuration/Storage-AzureBlob.md) | [Amazon S3](./Configuration/Storage-S3.md) | [Cloudflare R2](./Configuration/Storage-Cloudflare.md) | [Google Cloud](./Configuration/Storage-GoogleCloud.md)
- CDN: [Overview](./Configuration/CDN-Overview.md)
  - [Azure Front Door](./Configuration/CDN-AzureFrontDoor.md) | [Cloudflare](./Configuration/CDN-Cloudflare.md) | [CloudFront](./Configuration/CDN-CloudFront.md) | [Sucuri](./Configuration/CDN-Sucuri.md)

### Editing Tools Documentation
- Live Editor: [Complete Guide](./Editors/LiveEditor/README.md) | [Quick Start](./Editors/LiveEditor/QuickStart.md) | [Visual Guide](./Editors/LiveEditor/VisualGuide.md) | [Technical Reference](./Editors/LiveEditor/TechnicalReference.md)
- Designer: [Guide](./Editors/Designer/README.md) | [Quick Start](./Editors/Designer/QuickStart.md)
- Code Editor: [Guide](./Editors/CodeEditor/README.md)
- Image Editing: [Guide](./Editors/ImageEditing/README.md)
- File Management: [Overview](./FileManagement/index.md) | [Complete Guide](./FileManagement/README.md) | [Quick Start](./FileManagement/Quick-Start.md)

### Developer Documentation
- Controllers: [Overview](./Developers/Controllers/README.md) | [HomeControllerBase](./Developers/Controllers/HomeControllerBase.md) | [PubControllerBase](./Developers/Controllers/PubControllerBase.md)
- Widgets: [Overview](./Widgets/README.md)
  - [Image](./Widgets/Image-Widget.md) | [Crypto](./Widgets/Crypto-Widget.md) | [Crumbs](./Widgets/Crumbs-Widget.md) | [Forms](./Widgets/Forms-Widget.md) | [Nav Builder](./Widgets/Nav-Builder-Widget.md) | [Search](./Widgets/Search-Widget.md) | [ToC](./Widgets/ToC-Widget.md)
- Deep Dives: [Image Widget Development](./Developers/ImageWidget.md)

### Additional Guides
- [AWS S3 Access Keys Setup](./Configuration/AWS-S3-AccessKeys.md)
- [Cloudflare R2 Access Keys Setup](./Configuration/Cloudflare-R2-AccessKeys.md)
- [Uploading Secrets to GitHub Repository](./UploadSecretsToGithubRepo.md)

</details>
---

## Documentation Conventions

Throughout this documentation:

- **Bold** - Important terms and UI elements
- `Code blocks` - File names, commands, and code snippets
- **Icons** - Visual organization and quick reference
- [Links] - Cross-references to related documentation

---

## Contributing to Documentation

Want to improve these docs? See **[CONTRIBUTING.md](./CONTRIBUTING.md)** for:
- Documentation ownership model
- File naming conventions and standards
- Update triggers and review schedule
- Writing best practices and style guide
- How to submit documentation changes

---

**Last Updated:** December 15, 2025  
**Maintained by:** Moonrise Software, LLC  
**License:** See [LICENSE-GPL](../LICENSE-GPL) and [LICENSE-MIT](../LICENSE-MIT)

For project overview and "what is SkyCMS," see the [main README](../README.md).
