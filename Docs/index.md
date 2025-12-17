---
title: SkyCMS Documentation
description: Complete documentation hub for SkyCMS - installation, configuration, development, and deployment guides
keywords: documentation, index, guides, reference, installation, configuration
audience: [all]
---

# SkyCMS Documentation

**Welcome to the SkyCMS documentation hub.** This page provides organized access to all SkyCMS documentation, guides, and resources.

For a project overview and introduction to SkyCMS, see the [main README](https://github.com/CWALabs/SkyCMS#readme).

**Documentation Version:** 2.0 (December 2025)  
**Compatible with:** SkyCMS v2.x  
**Last Updated:** December 17, 2025

> **New to SkyCMS?** Check out our [**Learning Paths Guide**](./LEARNING_PATHS.html) for role-based documentation journeys!

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

- [Content Editor (Non-Technical)](./LEARNING_PATHS.html#-content-editor-non-technical) - 30-45 minutes
- [Developer](./LEARNING_PATHS.html#-developer) - 2-3 hours
- [DevOps / System Administrator](./LEARNING_PATHS.html#️-devops--system-administrator) - 3-4 hours
- [Decision Maker / Manager](./LEARNING_PATHS.html#-decision-maker--manager) - 30-45 minutes

See the complete [Learning Paths Guide](./LEARNING_PATHS.html) for detailed step-by-step instructions.

---

## Getting Started

Start here if you're new to SkyCMS.

- **Start with the [Learning Paths](./LEARNING_PATHS.html)** for a role-based guided journey
- **[Quick Start Guide](./QuickStart.html)** - Get up and running quickly
- **[About SkyCMS](./About.html)** - What SkyCMS is and who it's for
- **[Developer Experience](./DeveloperExperience.html)** - Overview for developers
- **[Migrating from JAMstack](./MigratingFromJAMstack.html)** - Moving from Git-based static site workflows

---

## Installation & Deployment

### Getting Started with Installation

Choose your configuration approach:
- 🧙 **Setup Wizard (guided)** - Minimal pre-reqs; configure interactively; best for first-time and single-tenant setups
- ⚙️ **Environment variables (automated)** - Pre-configure for Docker/Kubernetes/CI/CD; optionally run wizard for remaining settings
- 🏢 **Multi-tenant** - Wizard not supported; follow [Multi-Tenant Configuration](./Configuration/Multi-Tenant-Configuration.html) for domain-based tenant setup

- **[Installation Overview](./Installation/README.html)** - Choose your deployment platform
- **[Minimum Required Settings](./Installation/MinimumRequiredSettings.html)** - Essential configuration before deployment
- **[Setup Wizard Guide](./Installation/SetupWizard.html)** - Step-by-step interactive configuration
  - [Welcome Screen](./Installation/SetupWizard-Welcome.html)
  - [Step 1: Storage Configuration](./Installation/SetupWizard-Step1-Storage.html)
  - [Step 2: Admin Account](./Installation/SetupWizard-Step2-Admin.html)
  - [Step 3: Publisher Settings](./Installation/SetupWizard-Step3-Publisher.html)
  - [Step 4: Email Configuration](./Installation/SetupWizard-Step4-Email.html)
  - [Step 5: CDN Configuration](./Installation/SetupWizard-Step5-CDN.html)
  - [Step 6: Review & Complete](./Installation/SetupWizard-Step6-Review.html)
  - [Setup Complete](./Installation/SetupWizard-Complete.html)
- **[Post-Installation Configuration](./Installation/Post-Installation.html)** ⭐ - After setup wizard completes

### Cloud Platform Guides

- **[Azure Installation](./Installation/AzureInstall.html)** - Deploy SkyCMS to Microsoft Azure
- **[AWS S3 Static Website Hosting](./S3StaticWebsite.html)** - Deploy using S3 static hosting
- **[Cloudflare Edge Hosting](./Installation/CloudflareEdgeHosting.html)** - Origin-less hosting with Cloudflare R2 + Rules

---

## Configuration

Essential configuration guides for databases, storage, and CDN.

- **[Configuration Overview](./Configuration/README.html)** - Quick reference for all configuration documentation
- **[Database Configuration](./Configuration/Database-Overview.html)** - Supported providers, connection string formats, and setup steps
  - [Azure Cosmos DB](./Configuration/Database-CosmosDB.html)
  - [MS SQL Server / Azure SQL](./Configuration/Database-SQLServer.html)
  - [MySQL](./Configuration/Database-MySQL.html)
  - [SQLite](./Configuration/Database-SQLite.html)
- **[Storage Configuration](./Configuration/Storage-Overview.html)** - Supported providers, connection string formats, and setup steps
  - [Azure Blob Storage](./Configuration/Storage-AzureBlob.html)
  - [Amazon S3](./Configuration/Storage-S3.html)
  - [Cloudflare R2](./Configuration/Storage-Cloudflare.html)
  - [Google Cloud Storage](./Configuration/Storage-GoogleCloud.html)
- **[CDN Integration](./Configuration/CDN-Overview.html)** - Supported providers, required values, and where to configure in SkyCMS
  - [Azure Front Door CDN](./Configuration/CDN-AzureFrontDoor.html)
  - [Cloudflare CDN](./Configuration/CDN-Cloudflare.html)
  - [Amazon CloudFront CDN](./Configuration/CDN-CloudFront.html)
  - [Sucuri CDN/WAF](./Configuration/CDN-Sucuri.html)
- **[Email Configuration](./Configuration/Email-Overview.html)** - Email providers for transactional messages
  - [SendGrid](./Configuration/Email-SendGrid.html)
  - [Azure Communication Services](./Configuration/Email-AzureCommunicationServices.html)
  - [SMTP](./Configuration/Email-SMTP.html)
  - [No-Op (Development)](./Configuration/Email-None.html)
- **Reference Guides**
  - [Database Configuration Reference](./Configuration/Database-Configuration-Reference.html)
  - [Storage Configuration Reference](./Configuration/Storage-Configuration-Reference.html)
  - [Email Configuration Reference](./Configuration/Email-Configuration-Reference.html)
  - [CDN Configuration Reference](./Configuration/CDN-Configuration-Reference.html)

---

## Authentication & Authorization

Secure access and permission management for editors and users.

- **[Authentication Overview](./Authentication-Overview.html)** - Authentication methods, concepts, and setup guide
  - Local username/password authentication
  - Azure Active Directory (Azure AD)
  - Azure B2C for consumer identity
  - OpenID Connect / OAuth 2.0 for custom providers
- **[Identity Framework](./Components/AspNetCore.Identity.FlexDb.html)** - Flexible identity system for multiple databases
- **[Role-Based Access Control (RBAC) & Authorization](./Administration/Roles-and-Permissions.html)** ⭐ - Role definitions, permission matrix, and authorization management

---

## Content Management

### Page & Layout Management

- **[Layouts Guide](./Layouts/Readme.html)** - Creating and managing site-wide layouts (headers, footers, site structure)
- **[Page Templates Guide](./Templates/Readme.html)** - Reusable page structures and managing template-based pages
- **[Page Scheduling](./Editors/PageScheduling.html)** - Schedule pages for automatic future publication
- **[Migration: Save Article Pipeline](./MIGRATION-SAVE-ARTICLE.html)** - Internals of article persistence and versioning

### File & Media Management

- **[File Management Overview](./FileManagement/index.html)** - Entry point for file and media documentation
- **[Complete Guide](./FileManagement/README.html)** - Comprehensive file management reference
- **[Quick Start](./FileManagement/Quick-Start.html)** - Get started in 5-10 minutes

---

## Publishing

Publishing workflows, modes, and best practices.

- **[Publishing Overview](./Publishing-Overview.html)** - Publishing modes, workflows, and strategies
  - Direct publishing for rapid updates
  - Staged publishing for review workflows
  - Static generation for JAMstack deployment
  - Git-based publishing for CI/CD pipelines
- **[Scheduled Publishing](./Editors/PageScheduling.html)** - Schedule content for automatic publication

---

## Blogging

Comprehensive guides for SkyCMS blogging features.

- **[Blog Post Lifecycle](./blog/BlogPostLifecycle.html)** - Creating, editing, and publishing blog posts
- **[Future Blog Enhancements](./blog/BlogFutureEnhancements.html)** - Upcoming blog functionality and roadmap

---

## Editing Tools

SkyCMS integrates multiple powerful editors for different workflows.

### Live Editor (CKEditor 5)

WYSIWYG editing for content creators with inline editing capabilities.

- **[Complete Guide](./Editors/LiveEditor/README.html)** - Full feature documentation and reference
- **[Quick Start](./Editors/LiveEditor/QuickStart.html)** - Get started in 5 minutes
- **[Visual Guide](./Editors/LiveEditor/VisualGuide.html)** - Interface diagrams and visual reference
- **[Technical Reference](./Editors/LiveEditor/TechnicalReference.html)** - Developer documentation

### Designer (GrapesJS)

Visual drag-and-drop page builder for creating layouts without code.

- **[Designer Guide](./Editors/Designer/README.html)** - Complete documentation
- **[Quick Start](./Editors/Designer/QuickStart.html)** - Get started quickly

### Code Editor (Monaco)

Professional code editor for HTML, CSS, and JavaScript with syntax highlighting and IntelliSense.

- **[Code Editor Guide](./Editors/CodeEditor/README.html)** - Complete documentation

### Image Editing (Filerobot)

Integrated browser-based image editor for cropping, resizing, and adjusting images.

- **[Image Editing Guide](./Editors/ImageEditing/README.html)** - Complete documentation

---

## Widgets

Reusable UI components for content and developers.

- **[Widgets Overview](./Widgets-Overview.html)** - Understanding widgets and when to use them
- **[Widgets Directory](./Widgets/README.html)** - Complete widget documentation and API reference
  - [Image Widget](./Widgets/Image-Widget.html) - Image upload and management
  - [Crypto Widget](./Widgets/Crypto-Widget.html) - Encryption/decryption helpers
  - [Crumbs Widget](./Widgets/Crumbs-Widget.html) - Breadcrumb navigation
  - [Forms Widget](./Widgets/Forms-Widget.html) - Form handling and submissions
  - [Nav Builder Widget](./Widgets/Nav-Builder-Widget.html) - Navigation menu generation
  - [Search Widget](./Widgets/Search-Widget.html) - Full-text search functionality
  - [ToC Widget](./Widgets/ToC-Widget.html) - Table of contents generation

---

## Developer Workflows

Complete workflows and best practices for developers building and launching SkyCMS websites.

### **Website Launch Workflow** ⭐

**[Complete Website Launch Guide →](./Developer-Guides/Website-Launch-Workflow.html)**

A comprehensive 6-phase roadmap for taking a fresh SkyCMS installation to a fully functional website ready for content creators:

- **Phase 1:** Design & Planning - Site structure and architecture
- **Phase 2:** Creating Layouts - Site-wide structure (header, footer, nav)
- **Phase 3:** Creating Templates - Reusable page types
- **Phase 4:** Building Home Page - First live page and publishing
- **Phase 5:** Building Initial Pages - Content pages and navigation
- **Phase 6:** Preparing for Handoff - Team setup and training

**Supporting Resources:**
- [Pre-Launch Checklist](./Developer-Guides/Checklists/Pre-Launch-Checklist.html) - Verification before launch
- [Content Creator Onboarding](./Developer-Guides/Checklists/Content-Creator-Onboarding.html) - Team handoff
- [Monthly Maintenance Tasks](./Checklists/Monthly-Maintenance.html) - Ongoing upkeep
- [Style Guide Template](./Templates/Style-Guide-Template.html) - Brand guidelines
- [Training Document Template](./Templates/Training-Document-Template.html) - Team training
- [Content Workflow Template](./Templates/Content-Workflow-Template.html) - Approval processes

> **SkyCMS & Modern Approach**
>
> SkyCMS promotes modern practices: componentized layouts/templates, content-first workflows (draft→review→publish), separation of concerns, and baked-in quality (accessibility, performance, SEO). See the deep-dive: [SkyCMS & Modern Approach](./Developer-Guides/SkyCMS-Modern-Approach.html).

> **Comparisons**
>
> Evaluating SkyCMS against alternatives? See our comprehensive [Comparisons Matrix](./Comparisons.html) and [Developer Experience Comparison](./Developer-Experience-Comparison.html).

**Time to complete:** 35-55 hours over 2-3 weeks

**For Developers:** Full development workflow from scratch to handoff  
**For Designers:** Site structure and template planning  
**For Project Managers:** Timeline and milestone tracking

---

## For Developers

Technical documentation for customization and development.

### Developer Guides & Workflows

- **[Developer Guides Hub](./Developer-Guides/README.html)** - Overview of all developer workflows
- **[Website Launch Workflow](./Developer-Guides/Website-Launch-Workflow.html)** - Complete 6-phase project workflow

### Controllers & Base Classes

- **[Developer Overview](./Developers/README.html)** - Developer documentation hub
- **[HomeControllerBase](./Developers/Controllers/HomeControllerBase.html)** - Base controller for content pages
- **[PubControllerBase](./Developers/Controllers/PubControllerBase.html)** - Base controller for public-facing pages

### Widgets & Components

- **[Widgets Overview](./Widgets/README.html)** - Reusable UI components and widgets
  - [Image Widget](./Widgets/Image-Widget.html) - Image upload and management
  - [Crypto Widget](./Widgets/Crypto-Widget.html) - AES encryption/decryption helpers
  - [Crumbs Widget](./Widgets/Crumbs-Widget.html) - Breadcrumb navigation
  - [Forms Widget](./Widgets/Forms-Widget.html) - Form handling and antiforgery tokens
  - [Nav Builder Widget](./Widgets/Nav-Builder-Widget.html) - Navigation list rendering
  - [Search Widget](./Widgets/Search-Widget.html) - Search form and results
  - [ToC Widget](./Widgets/ToC-Widget.html) - Table of contents generation

### Additional Developer Resources

- **[Image Widget Development](./Developers/ImageWidget.html)** - Deep dive into image widget implementation

---

## Architecture & Components

Learn about SkyCMS's internal structure and libraries.

### Application Architecture

- **[Startup Lifecycle](./Architecture/Startup-Lifecycle.html)** ⭐ - Service registration, startup sequence, and initialization order
- **[Middleware Pipeline](./Architecture/Middleware-Pipeline.html)** ⭐ - Request processing, middleware execution order, and custom middleware

### Core Applications

- **[Editor Application](https://github.com/CWALabs/SkyCMS/tree/main/Editor#readme)** - Content management interface
- **[Publisher Application](https://github.com/CWALabs/SkyCMS/tree/main/Publisher#readme)** - Public-facing website renderer

### Component Libraries

- **[Common Library](./Components/Cosmos.Common.html)** - Core CMS library ([full docs](https://github.com/CWALabs/SkyCMS/tree/main/Common#readme))
- **[Blob Service](./Components/Cosmos.BlobService.html)** - Multi-cloud storage ([full docs](https://github.com/CWALabs/SkyCMS/tree/main/Cosmos.BlobService#readme))
- **[Identity Framework](./Components/AspNetCore.Identity.FlexDb.html)** - Flexible database identity ([full docs](https://github.com/CWALabs/SkyCMS/tree/main/AspNetCore.Identity.FlexDb#readme))
- **[Dynamic Configuration](https://github.com/CWALabs/SkyCMS/tree/main/Cosmos.ConnectionStrings#readme)** - Runtime configuration management

### Development & Testing

- **[Testing Guide](./Development/Testing/README.html)** - Comprehensive testing documentation

---

## Comparisons & Use Cases

Understand how SkyCMS compares to other solutions and make informed decisions.

- **[Comparisons Matrix](./Comparisons.html)** - Feature, cost, and workflow comparison with WordPress, Jekyll, Hugo, Gatsby, Contentful, Sanity, and more
- **[Developer Experience Comparison](./Developer-Experience-Comparison.html)** - Detailed workflow and learning curve comparison
- **[FAQ](./FAQ.html)** - Common questions about SkyCMS, comparisons, deployment, and team workflows
- **[Quick Reference](./Quick-Reference.html)** - One-page visual summary and cheat sheet
- **[SkyCMS vs Headless CMS](./CosmosVsHeadless.html)** - Understanding the edge-native CMS approach
- **[Migrating from JAMstack](./MigratingFromJAMstack.html)** - Moving from Git-based static site workflows

### Marketing & Competitive Materials

For detailed competitive analysis and cost comparisons, see the [Marketing Materials folder](./_Marketing/README.html):
- [Competitor Analysis](./_Marketing/Competitor-Analysis.html) - Detailed comparison with alternatives
- [Cost Comparison](./_Marketing/Cost-Comparison.html) - Operational cost analysis

---

## Troubleshooting

- **[Troubleshooting Guide](./Troubleshooting.html)** - Solutions for common setup and operational issues organized by feature area (database, storage, CDN, publishing, authentication, performance)

---

## Documentation Review & Gaps

- **[Documentation Gaps Analysis](./DOCUMENTATION_GAPS_ANALYSIS.html)** - Comprehensive review of documentation blind spots and missing topics
  - High-priority gaps (Post-Installation, Multi-Tenant, Startup Lifecycle)
  - Medium-priority gaps (Backup/Recovery, Monitoring, RBAC)
  - Architecture and API documentation gaps
  - Recommendations for documentation priorities

---

## Additional Resources

### Discovery & Analytics

- **[Sitemap](./sitemap.xml)** - XML sitemap for search engine crawlers
- **[Analytics Setup](./Analytics-Setup.html)** - Recommendations for tracking documentation engagement and user metrics
- **[JSON Feed](./docs-index.json)** - Machine-readable documentation index for AI crawlers and programmatic access

### Release Information

- **[Documentation Changelog](./CHANGELOG.html)** - Documentation updates and new content
- **[Version History](./CHANGELOG.html)** - SkyCMS release notes and version history

### Marketing & Sales Materials

See the [Marketing Materials folder](./_Marketing/README.html) for:
- Website content and homepage copy
- Azure Marketplace listing content
- Competitive analysis and positioning
- Cost comparisons with alternatives

### License & Legal

- **[License Information](./License.html)** - Dual licensing (GPL 2.0-or-later / MIT) details
- **[Third-Party Notices](https://github.com/CWALabs/SkyCMS/blob/main/NOTICE.md)** - Attribution and licenses for included components
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

For project overview and "what is SkyCMS," see the [main README](https://github.com/CWALabs/SkyCMS#readme).

---

## Complete Documentation Index

For an exhaustive list of all documentation files, see below:

<details>
<summary><b>Click to expand complete file listing</b></summary>

### Core Documentation
- Getting Started: [About SkyCMS](./About.html) | [Quick Start](./QuickStart.html) | [LEARNING_PATHS](./LEARNING_PATHS.html)
- Overview Documents: [Authentication](./Authentication-Overview.html) | [Publishing](./Publishing-Overview.html) | [Widgets](./Widgets-Overview.html)

### Configuration Guides
- Database: [Overview](./Configuration/Database-Overview.html) | [Reference](./Configuration/Database-Configuration-Reference.html)
  - [Azure Cosmos DB](./Configuration/Database-CosmosDB.html) | [MS SQL Server](./Configuration/Database-SQLServer.html) | [MySQL](./Configuration/Database-MySQL.html) | [SQLite](./Configuration/Database-SQLite.html)
- Storage: [Overview](./Configuration/Storage-Overview.html) | [Reference](./Configuration/Storage-Configuration-Reference.html)
  - [Azure Blob](./Configuration/Storage-AzureBlob.html) | [Amazon S3](./Configuration/Storage-S3.html) | [Cloudflare R2](./Configuration/Storage-Cloudflare.html) | [Google Cloud](./Configuration/Storage-GoogleCloud.html)
- CDN: [Overview](./Configuration/CDN-Overview.html)
  - [Azure Front Door](./Configuration/CDN-AzureFrontDoor.html) | [Cloudflare](./Configuration/CDN-Cloudflare.html) | [CloudFront](./Configuration/CDN-CloudFront.html) | [Sucuri](./Configuration/CDN-Sucuri.html)

### Editing Tools Documentation
- Live Editor: [Complete Guide](./Editors/LiveEditor/README.html) | [Quick Start](./Editors/LiveEditor/QuickStart.html) | [Visual Guide](./Editors/LiveEditor/VisualGuide.html) | [Technical Reference](./Editors/LiveEditor/TechnicalReference.html)
- Designer: [Guide](./Editors/Designer/README.html) | [Quick Start](./Editors/Designer/QuickStart.html)
- Code Editor: [Guide](./Editors/CodeEditor/README.html)
- Image Editing: [Guide](./Editors/ImageEditing/README.html)
- File Management: [Overview](./FileManagement/index.html) | [Complete Guide](./FileManagement/README.html) | [Quick Start](./FileManagement/Quick-Start.html)

### Developer Documentation
- Controllers: [Overview](./Developers/Controllers/README.html) | [HomeControllerBase](./Developers/Controllers/HomeControllerBase.html) | [PubControllerBase](./Developers/Controllers/PubControllerBase.html)
- Widgets: [Overview](./Widgets/README.html)
  - [Image](./Widgets/Image-Widget.html) | [Crypto](./Widgets/Crypto-Widget.html) | [Crumbs](./Widgets/Crumbs-Widget.html) | [Forms](./Widgets/Forms-Widget.html) | [Nav Builder](./Widgets/Nav-Builder-Widget.html) | [Search](./Widgets/Search-Widget.html) | [ToC](./Widgets/ToC-Widget.html)
- Deep Dives: [Image Widget Development](./Developers/ImageWidget.html)

### Additional Guides
- [AWS S3 Access Keys Setup](./Configuration/AWS-S3-AccessKeys.html)
- [Cloudflare R2 Access Keys Setup](./Configuration/Cloudflare-R2-AccessKeys.html)
- [Uploading Secrets to GitHub Repository](./UploadSecretsToGithubRepo.html)

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

Want to improve these docs? See **[CONTRIBUTING.md](./CONTRIBUTING.html)** for:
- Documentation ownership model
- File naming conventions and standards
- Update triggers and review schedule
- Writing best practices and style guide
- How to submit documentation changes

---

**Last Updated:** December 17, 2025  
**Maintained by:** Moonrise Software, LLC  
**License:** See [LICENSE-GPL](../LICENSE-GPL) and [LICENSE-MIT](../LICENSE-MIT)

For project overview and "what is SkyCMS," see the [main README](https://github.com/CWALabs/SkyCMS#readme).
