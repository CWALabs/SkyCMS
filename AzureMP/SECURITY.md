# Security Policy

## Supported Versions

We release patches for security vulnerabilities for the following versions:

| Version | Supported          |
| ------- | ------------------ |
| 9.x.x   | :white_check_mark: |
| < 9.0   | :x:                |

## Reporting a Vulnerability

**Please do not report security vulnerabilities through public GitHub issues.**

Instead, please report security vulnerabilities via email to: **security@moonrise.net**

You should receive a response within 48 hours. If for some reason you do not, please follow up via email to ensure we received your original message.

Please include the following information in your report:

- Type of issue (e.g., buffer overflow, SQL injection, cross-site scripting, etc.)
- Full paths of source file(s) related to the manifestation of the issue
- The location of the affected source code (tag/branch/commit or direct URL)
- Any special configuration required to reproduce the issue
- Step-by-step instructions to reproduce the issue
- Proof-of-concept or exploit code (if possible)
- Impact of the issue, including how an attacker might exploit it

This information will help us triage your report more quickly.

## Security Best Practices for Deployment

### Authentication & Access Control

- **Use strong passwords**: Require minimum 12 characters with complexity requirements
- **Enable multi-factor authentication**: Configure OAuth providers (Microsoft, Google) for admin accounts
- **Role-based access control**: Assign minimum necessary permissions to users
- **Azure B2C integration**: Use enterprise identity management for production deployments

### Data Protection

- **Encrypt data in transit**: SkyCMS enforces HTTPS for all connections
- **Encrypt data at rest**: Enable encryption on Azure Storage accounts and databases
- **Secure secrets management**: Use Azure Key Vault for connection strings and API keys
- **Never commit secrets**: Use environment variables and Azure App Configuration

### Database Security

- **Use managed identities**: Enable passwordless authentication between services
- **Network isolation**: Configure firewall rules to restrict database access
- **Regular backups**: Enable automated backups for Azure SQL/MySQL/Cosmos DB
- **TLS enforcement**: Require encrypted connections to databases

### Container Security

- **Use official images**: Deploy from verified Docker Hub images (toiyabe/sky-editor, toiyabe/sky-publisher)
- **Keep images updated**: Regularly update to latest versions for security patches
- **Scan for vulnerabilities**: Use Azure Defender for container image scanning
- **Run as non-root**: Container images run with non-privileged users

### Azure-Specific Security

- **Enable Azure Defender**: Activate threat protection for App Services and databases
- **Use Private Endpoints**: Configure private networking for production databases
- **Enable diagnostic logging**: Forward logs to Azure Monitor and Log Analytics
- **Configure WAF**: Use Azure Front Door or Application Gateway with Web Application Firewall

### Content Security

- **Input validation**: SkyCMS validates and sanitizes all user inputs
- **XSS protection**: Content Security Policy headers prevent cross-site scripting
- **CSRF protection**: Anti-forgery tokens protect form submissions
- **File upload restrictions**: Validate file types and sizes on uploads

## Known Security Considerations

### CKEditor 5 Licensing

SkyCMS includes CKEditor 5, which is licensed under GPL 2.0-or-later. Commercial use requires either:
- GPL compliance (open-source your application), or
- Purchase a commercial CKEditor license from CKSource

See [LICENSE-GPL](LICENSE-GPL) and [LICENSE-MIT](LICENSE-MIT) for details.

### Third-Party Dependencies

SkyCMS relies on several third-party components. We monitor security advisories for:
- CKEditor 5
- GrapesJS
- Monaco Editor
- ASP.NET Core and .NET libraries
- Docker base images

Run `dotnet list package --vulnerable` to check for vulnerable NuGet packages.

## Security Updates

Security updates are released as soon as possible after vulnerabilities are confirmed:

- **Critical**: Released within 24-48 hours
- **High**: Released within 1 week
- **Medium**: Included in next scheduled release
- **Low**: Included in future releases

Subscribe to our [GitHub releases](https://github.com/CWALabs/SkyCMS/releases) to receive notifications.

## Compliance

SkyCMS is designed to support compliance with:

- **GDPR**: Personal data handling controls, data export, right to deletion
- **SOC 2**: When deployed on Azure with appropriate controls
- **HIPAA**: Can be deployed in HIPAA-compliant Azure configurations
- **PCI DSS**: Not recommended for storing payment card data directly

**Note**: Compliance responsibility is shared between SkyCMS (application security) and your deployment configuration (infrastructure security).

## Security Audits

We welcome independent security audits. Please contact us at security@moonrise.net to coordinate responsible disclosure.

## Bug Bounty Program

We do not currently have a formal bug bounty program, but we recognize and appreciate security researchers who responsibly disclose vulnerabilities.

---

**Last Updated**: December 2025
