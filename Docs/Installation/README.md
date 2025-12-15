# SkyCMS Installation Guide

Choose your deployment platform below. All deployments follow the same core setup process:

1. **Configure minimum required settings** (database, storage connection strings)
2. **Run the setup wizard** (single-tenant) or configure tenants (multi-tenant)
3. **Create content and publish**

---

## Quick Start: Minimum Required Settings

Before deploying to any platform, review the [Minimum Required Settings](./MinimumRequiredSettings.md) guide. This covers:
- Single-tenant vs. multi-tenant configuration
- Required connection strings
- Environment variables and secrets management
- Configuration examples

**Start here** if you're setting up SkyCMS for the first time or need to understand the configuration options.

---

## Platform-Specific Guides

### Azure

[Azure Installation Guide](./AzureInstall.md)

Deploy SkyCMS to Microsoft Azure using the automated deployment template. This guide covers:
- Deploy-to-Azure button quick start
- Azure resource configuration
- Post-deployment setup
- Connecting your domain

**Best for**: Organizations already using Azure, need managed services, prefer Microsoft ecosystem.

---

### Cloudflare Edge Hosting

[Cloudflare Edge Hosting Guide](./CloudflareEdgeHosting.md)

Deploy a static website to Cloudflare using an origin-less (edge) architecture with R2 storage. This guide covers:
- Setting up Cloudflare R2 bucket
- Connecting SkyCMS to R2 storage
- Configuring custom domains and rules
- Edge-based request handling (no origin server)

**Best for**: Maximum performance, origin-less architecture, global edge distribution, pay-per-use pricing.

---

### AWS

[AWS Installation Guide](./AWSInstall.md)

Deploy SkyCMS on AWS with flexible hosting options. This guide covers:
- S3 bucket creation and configuration
- EC2, ECS, or Lightsail deployment options
- S3 static hosting for publishers
- CloudFront CDN integration
- IAM access key setup and permissions

**Best for**: Organizations already using AWS, need flexible infrastructure, prefer AWS services.

---

## Additional Resources

- [Configuration Overview](../Configuration/) - Detailed configuration options for databases, storage, and CDN
- [Troubleshooting](./MinimumRequiredSettings.md#troubleshooting) - Common installation issues and solutions
- [Developer Experience](../DeveloperExperience.md) - Development environment setup

---

## Next Steps After Installation

1. Access your SkyCMS Editor instance
2. Run the setup wizard (single-tenant) to configure:
   - Database connection
   - Storage provider (Azure Blob, S3, or Cloudflare R2)
   - Administrator account
   - Publisher settings
3. Create your first page and publish content
4. Configure your custom domain
5. Set up CDN for optimal performance (optional)

For more details, see [About SkyCMS](../About.md) and the [Complete Documentation Index](../).
