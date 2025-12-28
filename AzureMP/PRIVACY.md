# Privacy Policy for SkyCMS

**Last Updated**: December 28, 2025

## Introduction

This privacy policy describes how SkyCMS ("we", "our", or "the software") handles data when you deploy and use the SkyCMS content management system.

**Important**: SkyCMS is self-hosted software. You (the deployer) are the data controller responsible for compliance with privacy regulations. This document describes how SkyCMS handles data, not how any third-party service (like Azure, AWS, or Cloudflare) handles data.

## Data Collection by SkyCMS

### What SkyCMS Collects

SkyCMS collects and stores only the data necessary for content management functionality:

**User Account Data**
- Email addresses (for authentication and account management)
- Hashed passwords (using ASP.NET Core Identity encryption)
- User roles and permissions
- Account creation and last login timestamps

**Content Data**
- Articles, pages, and blog posts created by users
- Media files (images, documents) uploaded by users
- Page templates and layouts
- Website configuration settings
- Content version history

**System Logs**
- Application logs (errors, warnings, information)
- Content publishing events
- User authentication events
- System configuration changes

**Analytics Data** (Optional)
- If you enable analytics integration (Google Analytics, Azure Application Insights, etc.)
- Page view counts (stored in your database)
- No personal data is collected by SkyCMS for analytics—only aggregated counts

### What SkyCMS Does NOT Collect

- ❌ SkyCMS does not transmit any data to external servers (unless you configure third-party services)
- ❌ No telemetry is sent to the SkyCMS developers
- ❌ No tracking cookies are set by SkyCMS (third-party services you integrate may set their own)
- ❌ No behavioral analytics without your explicit configuration

## How SkyCMS Uses Data

**Authentication & Authorization**
- User credentials authenticate access to the CMS
- Role assignments control content editing permissions

**Content Management**
- Content data powers your published websites
- Version history enables content rollback
- Media library organizes uploaded files

**System Operations**
- Logs help diagnose errors and performance issues
- Configuration settings customize behavior

## Data Storage

### Where Data is Stored

YOU control where all data is stored based on your deployment configuration:

**Database** (You choose)
- Azure Cosmos DB (NoSQL, globally distributed)
- Azure SQL Database (relational, Azure-hosted)
- MySQL (Azure, AWS, self-hosted)
- SQLite (local file, single-tenant only)

**File Storage** (You choose)
- Azure Blob Storage
- Amazon S3
- Cloudflare R2
- Local file system

**Logs** (Optional, you configure)
- Azure Application Insights
- Azure Log Analytics
- AWS CloudWatch
- Local file logs

### Data Encryption

**In Transit**
- ✅ SkyCMS enforces HTTPS for all web connections
- ✅ Database connections use TLS/SSL encryption (when configured)
- ✅ Storage connections are encrypted (Azure/AWS default)

**At Rest**
- ✅ Azure Storage encryption enabled by default
- ✅ Azure SQL and Cosmos DB encrypt data at rest
- ✅ AWS S3 supports encryption at rest
- ⚠️ **Your responsibility**: Enable encryption on your chosen database and storage

### Data Retention

SkyCMS retains data indefinitely unless YOU:
- Delete content through the CMS interface
- Delete user accounts (via admin panel or database)
- Configure automatic cleanup policies
- Delete the entire database or storage containers

**Deleted Content**: When you delete content through SkyCMS:
- Content is removed from the database
- Associated files are deleted from storage
- Version history is also deleted
- No "soft delete" or recovery after database deletion (unless you've configured backups)

## Your Rights (Data Subject Rights)

As the deployer of SkyCMS, YOU are responsible for honoring data subject rights under GDPR, CCPA, and other regulations:

**Right to Access**
- Users can request their data via your admin panel
- Export user data via database queries

**Right to Deletion**
- Delete users through admin interface
- Data is permanently removed from the database

**Right to Portability**
- Export content as JSON or database dump
- No vendor lock-in; standard formats used

**Right to Rectification**
- Users can update their account information
- Admins can edit user data via admin panel

SkyCMS provides the tools; YOU must implement policies and procedures.

## Third-Party Services

SkyCMS does NOT connect to third-party services by default. YOU may optionally configure:

**Authentication Providers** (OAuth)
- Microsoft Account (Azure AD/Entra ID)
- Google OAuth
- Azure Active Directory B2C

**Email Services**
- Azure Communication Services
- SendGrid
- SMTP providers

**Analytics**
- Google Analytics (if you add tracking code)
- Azure Application Insights (if you configure)

**CDN/Caching**
- Cloudflare
- Azure CDN / Front Door
- AWS CloudFront

**Each third-party service has its own privacy policy**. Review them before integrating.

## Cookies

SkyCMS uses cookies only for essential functionality:

**Essential Cookies** (Cannot be disabled)
- `.AspNetCore.Antiforgery.*` - CSRF protection
- `.AspNetCore.Identity.Application` - User authentication session
- `ai_session` (if Application Insights enabled) - Performance monitoring

**No Advertising or Tracking Cookies** are set by SkyCMS.

If you integrate third-party analytics or advertising, those services may set additional cookies.

## Children's Privacy

SkyCMS is not directed at children under 13 years of age. We do not knowingly collect personal information from children. If you deploy SkyCMS and allow public registrations, YOU are responsible for age verification and parental consent.

## GDPR Compliance

SkyCMS provides features to help with GDPR compliance:

✅ **Data Minimization**: Only collects necessary data
✅ **Encryption**: Supports encrypted storage and transit
✅ **Access Controls**: Role-based permissions
✅ **Data Portability**: Export content as JSON
✅ **Right to Deletion**: Delete users and content
✅ **Audit Logs**: Track content changes and user actions

**Your responsibilities as data controller**:
- Obtain consent for data collection (if required)
- Provide privacy notice to your users
- Honor data subject requests (access, deletion, etc.)
- Implement appropriate security measures
- Report data breaches (if applicable)

## CCPA Compliance

For California residents, SkyCMS enables you to:

- Disclose categories of personal information collected
- Allow users to request deletion of their data
- Allow users to opt-out of "sale" (though SkyCMS doesn't sell data)

## International Data Transfers

SkyCMS runs on YOUR infrastructure. Data residency depends on YOUR deployment:

- Deploy in EU regions for EU data residency
- Deploy in US regions for US data residency
- Azure, AWS, and other clouds offer region selection

YOU are responsible for compliance with data transfer regulations.

## Security

See [SECURITY.md](./SECURITY.md) for detailed security practices.

**In summary**:
- Passwords are hashed (ASP.NET Core Identity)
- HTTPS enforced for all connections
- SQL injection protection via Entity Framework
- XSS protection via Content Security Policy
- CSRF protection via anti-forgery tokens

## Changes to This Privacy Policy

We may update this privacy policy as SkyCMS evolves. Changes will be posted to the GitHub repository.

Check the "Last Updated" date at the top of this document.

## Data Controller Information

When you deploy SkyCMS, **YOU** are the data controller. You should provide your own privacy policy to your end users that includes:

- Your company/organization name
- Your contact information
- How you use SkyCMS data
- Your data retention policies
- How to submit privacy requests

## Contact

For questions about SkyCMS privacy practices:
- Email: privacy@moonrise.net
- GitHub Issues: [SkyCMS Issues](https://github.com/CWALabs/SkyCMS/issues)

For data subject requests, contact the organization that deployed SkyCMS (not the SkyCMS developers).

---

**Disclaimer**: This privacy policy describes SkyCMS software behavior. It is not legal advice. Consult with legal counsel for privacy compliance in your jurisdiction.

**Copyright (c) 2025 Moonrise Software, LLC**
