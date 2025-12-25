---
audience: [developers, administrators]
title: Installation Guide
description: Complete installation guide for SkyCMS including wizard setup and manual configuration options
keywords: installation, setup, Docker, AWS, Azure, configuration, wizard
audience: [developers, administrators]
version: 2.0
updated: 2025-12-20
canonical: /Installation/README.html
aliases: []
scope:
   platforms: [azure, aws, cloudflare, local]
   tenancy: [single, multi]
status: stable
chunk_hint: 340
---

# SkyCMS Installation Guide

## Key facts {#key-facts}

- Two config paths: single-tenant wizard (CosmosAllowSetup=true) vs env-var-driven (production); multi-tenant uses DynamicConfig, not the wizard.
- Minimum Required Settings are the source of truth for env vars and connection strings—do that before platform-specific guides.
- Azure/AWS/Cloudflare guides assume provider roles are set; use provider CLIs to validate connectivity before running SkyCMS.
- Post-install checklist: run Setup Wizard (single-tenant), verify publish, email, CDN, and create first admin.

## Choose Your Configuration Approach {#choose-approach}

SkyCMS offers two ways to configure your installation:

### 🧙 **Interactive Wizard (Recommended for New Users)** {#wizard}
- Minimal pre-configuration (just database + enable wizard)
- Step-by-step guided setup through web UI
- Configure storage, admin account, publisher, email, CDN interactively
- **Best for**: First-time installations, development, testing
- **Start here**: [Setup Wizard Guide](./SetupWizard.md)

### ⚙️ **Environment Variables (Recommended for Production)** {#env-vars}
- Pre-configure all settings via environment variables
- Optional: use wizard to configure remaining settings interactively
- Settings are read-only/hidden in wizard when pre-configured
- **Best for**: Docker/Kubernetes, CI/CD pipelines, production deployments
- **Start here**: [Minimum Required Settings](./MinimumRequiredSettings.md)

> **You can mix both approaches**: Pre-configure sensitive settings (database, storage credentials) via environment variables, then use the wizard for remaining settings (admin account, publisher URL, etc.).

---

## Quick Start: Minimum Required Settings {#minimum-settings}

Before deploying to any platform, review the [Minimum Required Settings](./MinimumRequiredSettings.md) guide. This covers:
- Single-tenant vs. multi-tenant configuration
- Required connection strings
- Environment variables and secrets management
- Configuration examples

**Start here** if you're setting up SkyCMS for the first time or need to understand the configuration options.

### After Installation Completes {#after-installation}

Once setup is finished, follow the **[Post-Installation Configuration Guide](./Post-Installation.md)** to:
- Verify your installation is fully operational
- Create and publish your first page
- Configure security and access control
- Test email and CDN integration
- Set up user accounts for your team

---

## Platform-Specific Guides {#platform-guides}

### Azure {#azure}

[Azure Installation Guide](./AzureInstall.md)

Deploy SkyCMS to Microsoft Azure using the automated deployment template. This guide covers:
- Deploy-to-Azure button quick start
- Azure resource configuration
- Post-deployment setup
- Connecting your domain

**Best for**: Organizations already using Azure, need managed services, prefer Microsoft ecosystem.

---

### AWS {#aws}

[AWS Installation Guide](./AWSInstall.md)

Deploy SkyCMS on AWS with flexible hosting options. This guide covers:
- S3 bucket creation and configuration
- EC2, ECS, or Lightsail deployment options
- S3 static hosting for publishers
- CloudFront CDN integration
- IAM access key setup and permissions

**Best for**: Organizations already using AWS, need flexible infrastructure, prefer AWS services.

---

### Cloudflare Edge Hosting {#cloudflare}

[Cloudflare Edge Hosting Guide](./CloudflareEdgeHosting.md)

Deploy a static website to Cloudflare using an origin-less (edge) architecture with R2 storage. This guide covers:
- Setting up Cloudflare R2 bucket
- Connecting SkyCMS to R2 storage
- Configuring custom domains and rules
- Edge-based request handling (no origin server)

**Best for**: Maximum performance, origin-less architecture, global edge distribution, pay-per-use pricing.

---

## Additional Resources {#additional-resources}

- [Configuration Overview](../Configuration/) - Detailed configuration options for databases, storage, and CDN
- [Troubleshooting](./MinimumRequiredSettings.md#troubleshooting) - Common installation issues and solutions
- [Developer Experience](../DeveloperExperience.md) - Development environment setup

---

## Next Steps After Installation {#next-steps}

1. Access your SkyCMS Editor instance
2. Run the **[Setup Wizard](./SetupWizard.md)** (single-tenant) to configure:
   - Database connection
   - Storage provider (Azure Blob, S3, R2, or Google Cloud Storage)
   - Administrator account
   - Publisher settings
   - Email configuration (optional)
   - CDN configuration (optional)
3. Create your first page and publish content
4. Configure your custom domain
5. Set up CDN for optimal performance (optional)

See the **[Setup Wizard Guide](./SetupWizard.md)** for step-by-step instructions with dedicated pages for each wizard step.

For more details, see [About SkyCMS](../About.md) and the [Complete Documentation Index](../).

## FAQ {#faq}

- **Should I use the wizard in production?** Only for single-tenant quick starts. For production, preconfigure via environment variables; for multi-tenant, disable the wizard and use DynamicConfig.
- **Which database should I start with locally?** SQLite for local demos; Azure SQL or MySQL for shared and production setups.
- **How do I verify connectivity before running SkyCMS?** Use provider CLIs (az, aws, cloudflare, mysql/sqlcmd) to test connection strings and permissions first.

<script type="application/ld+json">
{
   "@context": "https://schema.org",
   "@type": "FAQPage",
   "mainEntity": [
      {"@type": "Question", "name": "Should I use the wizard in production?", "acceptedAnswer": {"@type": "Answer", "text": "Use the wizard only for single-tenant quick starts. For production, preconfigure via environment variables; for multi-tenant, disable the wizard and use DynamicConfig."}},
      {"@type": "Question", "name": "Which database should I start with locally?", "acceptedAnswer": {"@type": "Answer", "text": "Use SQLite for local demos; use Azure SQL or MySQL for shared and production setups."}},
      {"@type": "Question", "name": "How do I verify connectivity before running SkyCMS?", "acceptedAnswer": {"@type": "Answer", "text": "Use provider CLIs such as az, aws, cloudflare, mysql, or sqlcmd to test connection strings and permissions before running SkyCMS."}}
   ]
}
</script>
