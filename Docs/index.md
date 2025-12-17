---
title: SkyCMS Documentation
description: Complete documentation hub for SkyCMS - installation, configuration, development, and deployment guides
keywords: documentation, index, guides, reference, installation, configuration
audience: [all]
---

# SkyCMS Documentation

**Welcome to the SkyCMS documentation hub.** This page provides organized access to all SkyCMS documentation, guides, and resources.

For a project overview and introduction to SkyCMS, see the [main README](../README.md).

**Documentation Version:** 2.0 (December 2025)  
**Compatible with:** SkyCMS v2.x  
**Last Updated:** December 17, 2025

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

- **Start with the [Learning Paths](./LEARNING_PATHS.md)** for a role-based guided journey
- **[Quick Start Guide](./QuickStart.md)** - Get up and running quickly
- **[About SkyCMS](./About.md)** - What SkyCMS is and who it's for
- **[Developer Experience](./DeveloperExperience.md)** - Overview for developers
- **[Migrating from JAMstack](./MigratingFromJAMstack.md)** - Moving from Git-based static site workflows

---

## Installation & Deployment

### Getting Started with Installation

Choose your configuration approach:
- 🧙 **Setup Wizard (guided)** - Minimal pre-reqs; configure interactively; best for first-time and single-tenant setups
- ⚙️ **Environment variables (automated)** - Pre-configure for Docker/Kubernetes/CI/CD; optionally run wizard for remaining settings
- 🏢 **Multi-tenant** - Wizard not supported; follow [Multi-Tenant Configuration](./Configuration/Multi-Tenant-Configuration.md) for domain-based tenant setup

- **[Installation Overview](./Installation/README.md)** - Choose your deployment platform
- **[Minimum Required Settings](./Installation/MinimumRequiredSettings.md)** - Essential configuration before deployment
- **[Setup Wizard Guide](./Installation/SetupWizard.md)** - Step-by-step interactive configuration
  - [Welcome Screen](./Installation/SetupWizard-Welcome.md)
  - [Step 1: Storage Configuration](./Installation/SetupWizard-Step1-Storage.md)
  - [Step 2: Admin Account](./Installation/SetupWizard-Step2-Admin.md)
  - [Step 3: Publisher Settings](./Installation/SetupWizard-Step3-Publisher.md)
  - [Step 4: Email Configuration](./Installation/SetupWizard-Step4-Email.md)
  - [Step 5: CDN Configuration](./Installation/SetupWizard-Step5-CDN.md)
  - [Step 6: Review & Complete](./Installation/SetupWizard-Step6-Review.md)
  - [Setup Complete](./Installation/SetupWizard-Complete.md)
- **[Post-Installation Configuration](./Installation/Post-Installation.md)** ⭐ - After setup wizard completes

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
- **[Email Configuration](./Configuration/Email-Overview.md)** - Email providers for transactional messages
  - [SendGrid](./Configuration/Email-SendGrid.md)
  - [Azure Communication Services](./Configuration/Email-AzureCommunicationServices.md)
  - [SMTP](./Configuration/Email-SMTP.md)
  - [No-Op (Development)](./Configuration/Email-None.md)
- **Reference Guides**
  - [Database Configuration Reference](./Configuration/Database-Configuration-Reference.md)
  - [Storage Configuration Reference](./Configuration/Storage-Configuration-Reference.md)
  - [Email Configuration Reference](./Configuration/Email-Configuration-Reference.md)
  - [CDN Configuration Reference](./Configuration/CDN-Configuration-Reference.md)

---

## Authentication & Authorization

Secure access and permission management for editors and users.

- **[Authentication Overview](./Authentication-Overview.md)** - Authentication methods, concepts, and setup guide
  - Local username/password authentication
  - Azure Active Directory (Azure AD)
  - Azure B2C for consumer identity
  - OpenID Connect / OAuth 2.0 for custom providers
- **[Identity Framework](./Components/AspNetCore.Identity.FlexDb.md)** - Flexible identity system for multiple databases
- **[Role-Based Access Control (RBAC) & Authorization](./Administration/Roles-and-Permissions.md)** ⭐ - Role definitions, permission matrix, and authorization management

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

## Developer Workflows

Complete workflows and best practices for developers building and launching SkyCMS websites.

### **Website Launch Workflow** ⭐

**[Complete Website Launch Guide →](./Developer-Guides/Website-Launch-Workflow.md)**

A comprehensive 6-phase roadmap for taking a fresh SkyCMS installation to a fully functional website ready for content creators:

- **Phase 1:** Design & Planning - Site structure and architecture
- **Phase 2:** Creating Layouts - Site-wide structure (header, footer, nav)
- **Phase 3:** Creating Templates - Reusable page types
- **Phase 4:** Building Home Page - First live page and publishing
- **Phase 5:** Building Initial Pages - Content pages and navigation
- **Phase 6:** Preparing for Handoff - Team setup and training

**Supporting Resources:**
- [Pre-Launch Checklist](./Developer-Guides/Checklists/Pre-Launch-Checklist.md) - Verification before launch
- [Content Creator Onboarding](./Developer-Guides/Checklists/Content-Creator-Onboarding.md) - Team handoff
- [Monthly Maintenance Tasks](./Checklists/Monthly-Maintenance.md) - Ongoing upkeep
- [Style Guide Template](./Templates/Style-Guide-Template.md) - Brand guidelines
- [Training Document Template](./Templates/Training-Document-Template.md) - Team training
- [Content Workflow Template](./Templates/Content-Workflow-Template.md) - Approval processes

> **SkyCMS & Modern Approach**
>
> SkyCMS promotes modern practices: componentized layouts/templates, content-first workflows (draft→review→publish), separation of concerns, and baked-in quality (accessibility, performance, SEO). See the deep-dive: [SkyCMS & Modern Approach](./Developer-Guides/SkyCMS-Modern-Approach.md).

> **Comparisons**
>
> Evaluating SkyCMS against alternatives? See our comprehensive [Comparisons Matrix](./Comparisons.md) and [Developer Experience Comparison](./Developer-Experience-Comparison.md).

**Time to complete:** 35-55 hours over 2-3 weeks

**For Developers:** Full development workflow from scratch to handoff  
**For Designers:** Site structure and template planning  
**For Project Managers:** Timeline and milestone tracking

---

## For Developers

Technical documentation for customization and development.

### Developer Guides & Workflows

- **[Developer Guides Hub](./Developer-Guides/README.md)** - Overview of all developer workflows
- **[Website Launch Workflow](./Developer-Guides/Website-Launch-Workflow.md)** - Complete 6-phase project workflow

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

### Application Architecture

- **[Startup Lifecycle](./Architecture/Startup-Lifecycle.md)** ⭐ - Service registration, startup sequence, and initialization order
- **[Middleware Pipeline](./Architecture/Middleware-Pipeline.md)** ⭐ - Request processing, middleware execution order, and custom middleware

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

Understand how SkyCMS compares to other solutions and make informed decisions.

- **[Comparisons Matrix](./Comparisons.md)** - Feature, cost, and workflow comparison with WordPress, Jekyll, Hugo, Gatsby, Contentful, Sanity, and more
- **[Developer Experience Comparison](./Developer-Experience-Comparison.md)** - Detailed workflow and learning curve comparison
- **[FAQ](./FAQ.md)** - Common questions about SkyCMS, comparisons, deployment, and team workflows
- **[Quick Reference](./Quick-Reference.md)** - One-page visual summary and cheat sheet
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

## Documentation Review & Gaps

- **[Documentation Gaps Analysis](./DOCUMENTATION_GAPS_ANALYSIS.md)** - Comprehensive review of documentation blind spots and missing topics
  - High-priority gaps (Post-Installation, Multi-Tenant, Startup Lifecycle)
  - Medium-priority gaps (Backup/Recovery, Monitoring, RBAC)
  - Architecture and API documentation gaps
  - Recommendations for documentation priorities

---

## Additional Resources

### Discovery & Analytics

- **[Sitemap](./sitemap.xml)** - XML sitemap for search engine crawlers
- **[Analytics Setup](./Analytics-Setup.md)** - Recommendations for tracking documentation engagement and user metrics
- **[JSON Feed](./docs-index.json)** - Machine-readable documentation index for AI crawlers and programmatic access

### Release Information

- **[Documentation Changelog](./CHANGELOG.md)** - Documentation updates and new content
- **[Version History](./CHANGELOG.md)** - SkyCMS release notes and version history

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

**Last Updated:** December 17, 2025  
**Maintained by:** Moonrise Software, LLC  
**License:** See [LICENSE-GPL](../LICENSE-GPL) and [LICENSE-MIT](../LICENSE-MIT)

For project overview and "what is SkyCMS," see the [main README](../README.md).
