---
title: SkyCMS Documentation
description: Fast, edge-native CMS for developers and content teams
keywords: documentation, cms, getting started
audience: [all]
---

# SkyCMS Documentation

<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "TechArticle",
  "headline": "SkyCMS Documentation Home",
  "description": "Fast, edge-native CMS for developers and content teams.",
  "author": { "@type": "Organization", "name": "CWALabs" },
  "dateModified": "2026-01-03",
  "version": "Docs 2.0 / SkyCMS v9.2x",
  "keywords": ["skycms", "documentation", "cms", "edge", "jamstack", "azure", "aws", "cloudflare"],
  "audience": ["all"],
  "inLanguage": "en",
  "url": "https://docs-sky-cms.com"
}
</script>

**SkyCMS is an edge-native CMS that combines the simplicity of WordPress with the performance of JAMstack.** Deploys to Azure, AWS, or other clouds—no complex build pipelines required.

**Version:** 2.0 (December 2025) | **Compatible with:** SkyCMS v9.2x

---

## When to use this
- You need a single starting point to navigate SkyCMS docs by role (creators, developers, decision makers, DevOps).
- You want the fastest path to install, launch, and compare SkyCMS without reading every page.

## Next steps
- Pick your lane below, or jump straight to the Quick Start / Installation guides linked in each section.

## Choose Your Path

### Content Creators
*Create and manage content without technical expertise*

**Start here:** [Quick Start for Editors](./QuickStart.html) (5 min)

- [Creating Your First Page](./Editors/LiveEditor/QuickStart.html)
- [Managing Files & Images](./FileManagement/Quick-Start.html)
- [Publishing & Scheduling](./Editors/PageScheduling.html)
- [Blog Management](./blog/BlogPostLifecycle.html)

**Editing Tools:**
- [Live Editor (WYSIWYG)](./Editors/LiveEditor/README.html)
- [Visual Designer (Drag & Drop)](./Editors/Designer/README.html)
- [Image Editing](./Editors/ImageEditing/README.html)

---

### Developers
*Build, customize, and deploy SkyCMS sites*

**Start here:** [Developer Quick Start](./QuickStart.html) (15 min)

**Essential Guides:**
- [Website Launch Workflow](./Developer-Guides/Website-Launch-Workflow.html) ⭐ (Complete project roadmap)
- [Installation Overview](./Installation/README.html)
- [Layouts & Templates](./Layouts/Readme.html)
- [Widgets & Components](./Widgets/README.html)

**Technical Deep Dives:**
- [Architecture](./Architecture/Startup-Lifecycle.html)
- [Controllers & Base Classes](./Developers/Controllers/README.html)
- [Testing Guide](./Development/Testing/README.html)

---

### Decision Makers
*Evaluate SkyCMS for your organization*

**Start here:** [About SkyCMS](./About.html) (10 min)

- [SkyCMS vs Alternatives](./Comparisons.html)
- [Developer Experience Comparison](./Developer-Experience-Comparison.html)
- [Total Cost of Ownership](./_Marketing/Cost-Comparison.html)
- [FAQ](./FAQ.html)

**Key Strengths:**
- No vendor lock-in (open source, dual-licensed)
- Deploy anywhere (Azure, AWS, Cloudflare)
- Modern architecture (edge-native, JAMstack-compatible)
- Familiar workflow (like WordPress, but faster)

---

### DevOps / System Administrators
*Deploy, configure, and maintain SkyCMS*

**Start here:** [Installation Overview](./Installation/README.html) (10 min)

**Deployment Guides:**
- [Azure Installation](./Installation/AzureInstall.html)
- [AWS S3 Static Hosting](./S3StaticWebsite.html)
- [Cloudflare Edge Hosting](./Installation/CloudflareEdgeHosting.html)

**Configuration:**
- [Setup Wizard Guide](./Installation/SetupWizard.html)
- [Database Configuration](./Configuration/Database-Overview.html)
- [Storage Configuration](./Configuration/Storage-Overview.html)
- [CDN Integration](./Configuration/CDN-Overview.html)
- [Multi-Tenant Setup](./Configuration/Multi-Tenant-Configuration.html)

**Operations:**
- [Post-Installation Tasks](./Installation/Post-Installation.html) ⭐
- [Monthly Maintenance](./Checklists/Monthly-Maintenance.html)
- [Troubleshooting](./Troubleshooting.html)

---

## Core Topics

### Authentication & Security
- [Authentication Overview](./Authentication-Overview.html)
- [Role-Based Access Control](./Administration/Roles-and-Permissions.html) ⭐
- [Identity Framework](./Components/AspNetCore.Identity.FlexDb.html)

### Content Management
- [Page Templates](./Templates/Readme.html)
- [Layouts](./Layouts/Readme.html)
- [File Management](./FileManagement/README.html)
- [Blog System](./blog/BlogPostLifecycle.html)

### Publishing
- [Publishing Modes](./Publishing-Overview.html)
- [Scheduled Publishing](./Editors/PageScheduling.html)
- [Static Generation](./Publishing-Overview.html)

### Integration
- [Email Configuration](./Configuration/Email-Overview.html)
- [CDN Integration](./Configuration/CDN-Overview.html)
- [Storage Providers](./Configuration/Storage-Overview.html)

---

## Popular Guides

| Getting Started | Migration | Reference |
|----------------|-----------|-----------|
| [Quick Start Guide](./QuickStart.html) | [From JAMstack](./MigratingFromJAMstack.html) | [Widgets Directory](./Widgets/README.html) |
| [Learning Paths](./LEARNING_PATHS.html) | [From WordPress](./FAQ.html#wordpress-migration) | [Configuration Reference](./Configuration/README.html) |
| [First Website Tutorial](./Developer-Guides/Website-Launch-Workflow.html) | [Setup Wizard](./Installation/SetupWizard.html) | [API Documentation](./Developers/README.html) |

---

## Need Help?

- **Can't find what you're looking for?** Try the [complete index](#complete-documentation-index) below
- **Report issues:** [GitHub Issues](https://github.com/CWALabs/SkyCMS/issues)
- **Contribute:** [Documentation Guidelines](./CONTRIBUTING.html)

---

## Complete Documentation Index

<details>
<summary><strong>Click to expand all documentation</strong></summary>

### Installation & Deployment
- [Installation Overview](./Installation/README.html)
- [Minimum Required Settings](./Installation/MinimumRequiredSettings.html)
- [Setup Wizard](./Installation/SetupWizard.html)
  - [Welcome](./Installation/SetupWizard-Welcome.html)
  - [Storage](./Installation/SetupWizard-Step1-Storage.html)
  - [Admin Account](./Installation/SetupWizard-Step2-Admin.html)
  - [Publisher](./Installation/SetupWizard-Step3-Publisher.html)
  - [Email](./Installation/SetupWizard-Step4-Email.html)
  - [CDN](./Installation/SetupWizard-Step5-CDN.html)
  - [Review](./Installation/SetupWizard-Step6-Review.html)
- [Post-Installation](./Installation/Post-Installation.html)
- Platform Guides:
  - [Azure](./Installation/AzureInstall.html)
  - [AWS S3](./S3StaticWebsite.html)
  - [Cloudflare Edge](./Installation/CloudflareEdgeHosting.html)

### Configuration
- [Database](./Configuration/Database-Overview.html): [Cosmos DB](./Configuration/Database-CosmosDB.html) | [SQL Server](./Configuration/Database-SQLServer.html) | [MySQL](./Configuration/Database-MySQL.html) | [SQLite](./Configuration/Database-SQLite.html)
- [Storage](./Configuration/Storage-Overview.html): [Azure Blob](./Configuration/Storage-AzureBlob.html) | [S3](./Configuration/Storage-S3.html) | [R2](./Configuration/Storage-Cloudflare.html) | [GCS](./Configuration/Storage-GoogleCloud.html)
- [CDN](./Configuration/CDN-Overview.html): [Azure Front Door](./Configuration/CDN-AzureFrontDoor.html) | [Cloudflare](./Configuration/CDN-Cloudflare.html) | [CloudFront](./Configuration/CDN-CloudFront.html) | [Sucuri](./Configuration/CDN-Sucuri.html)
- [Email](./Configuration/Email-Overview.html): [SendGrid](./Configuration/Email-SendGrid.html) | [Azure Communication](./Configuration/Email-AzureCommunicationServices.html) | [SMTP](./Configuration/Email-SMTP.html)
- [Multi-Tenant](./Configuration/Multi-Tenant-Configuration.html)

### Content Editing
- **Live Editor:** [Guide](./Editors/LiveEditor/README.html) | [Quick Start](./Editors/LiveEditor/QuickStart.html) | [Visual Guide](./Editors/LiveEditor/VisualGuide.html)
- **Designer:** [Guide](./Editors/Designer/README.html) | [Quick Start](./Editors/Designer/QuickStart.html)
- **Code Editor:** [Guide](./Editors/CodeEditor/README.html)
- **Image Editing:** [Guide](./Editors/ImageEditing/README.html)
- **File Management:** [Guide](./FileManagement/README.html) | [Quick Start](./FileManagement/Quick-Start.html)

### Developer Resources
- [Developer Hub](./Developers/README.html)
- [Website Launch Workflow](./Developer-Guides/Website-Launch-Workflow.html)
- Controllers: [HomeControllerBase](./Developers/Controllers/HomeControllerBase.html) | [PubControllerBase](./Developers/Controllers/PubControllerBase.html)
- Widgets: [Overview](./Widgets/README.html) | [Image](./Widgets/Image-Widget.html) | [Forms](./Widgets/Forms-Widget.html) | [Search](./Widgets/Search-Widget.html) | [Navigation](./Widgets/Nav-Builder-Widget.html)
- Architecture: [Startup](./Architecture/Startup-Lifecycle.html) | [Middleware](./Architecture/Middleware-Pipeline.html)
- [Testing Guide](./Development/Testing/README.html)

### Comparisons & Evaluation
- [Comparisons Matrix](./Comparisons.html)
- [Developer Experience Comparison](./Developer-Experience-Comparison.html)
- [SkyCMS vs Headless CMS](./CosmosVsHeadless.html)
- [Migrating from JAMstack](./MigratingFromJAMstack.html)
- [FAQ](./FAQ.html)

### Support & Maintenance
- [Troubleshooting](./Troubleshooting.html)
- [Documentation Gaps Analysis](./DOCUMENTATION_GAPS_ANALYSIS.html)
- [Changelog](./CHANGELOG.html)

</details>

---

## Additional Resources

- **[Sitemap](./sitemap.xml)** - XML sitemap for search engine crawlers
- **[JSON Feed](./docs-index.json)** - Machine-readable documentation index
- **[Analytics Setup](./Analytics-Setup.html)** - Documentation engagement tracking
- **[AI & SEO Content Standards](./AI-SEO-Content-Standards.html)** - Front matter conventions
- **[License Information](./License.html)** - Dual licensing (GPL 2.0-or-later / MIT)

---

**Last Updated:** December 17, 2025  
**Maintained by:** Moonrise Software, LLC  
**License:** See [LICENSE-GPL](../LICENSE-GPL) and [LICENSE-MIT](../LICENSE-MIT)

For project overview and "what is SkyCMS," see the [main README](https://github.com/CWALabs/SkyCMS#readme).
